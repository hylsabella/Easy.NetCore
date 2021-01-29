using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Easy.Common.NetCore.Helpers
{
    public static class EncryptionHelper
    {
        #region RSA

        /// <summary>
        /// RSA加密（兼容JavaScript互调）
        /// </summary>
        /// <param name="content">明文</param>
        /// <param name="publicKey">公钥Key</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>密文</returns>
        public static string RSA加密(string content, string publicKey, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            RSACryptoServiceProvider rsa = CreateRsaProviderFromPublicKey(publicKey);

            byte[] cipherbytes = rsa.Encrypt(encoding.GetBytes(content), false);

            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA解密（兼容JavaScript互调）
        /// </summary>
        /// <param name="content">密文</param>
        /// <param name="privateKey">私钥Key</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>明文</returns>
        public static string RSA解密(string content, string privateKey, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            RSACryptoServiceProvider rsa = CreateRsaProviderFromPrivateKey(privateKey);

            byte[] cipherbytes = rsa.Decrypt(Convert.FromBase64String(content), false);

            return encoding.GetString(cipherbytes);
        }

        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            var privateKeyBits = Convert.FromBase64String(privateKey);

            var rsaProvider = new RSACryptoServiceProvider();
            var rsaParams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                ushort twobytes = binr.ReadUInt16();

                if (twobytes == 0x8130)
                {
                    binr.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();
                }
                else
                {
                    throw new Exception("Unexpected value read binr.ReadUInt16()");
                }

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102) throw new Exception("Unexpected version");

                byte bt = binr.ReadByte();
                if (bt != 0x00) throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.D = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.P = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.Q = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.DP = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                rsaParams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));

                rsaProvider.ImportParameters(rsaParams);
            }

            return rsaProvider;
        }

        private static RSACryptoServiceProvider CreateRsaProviderFromPublicKey(string publickey)
        {
            var publickeyBits = Convert.FromBase64String(publickey);

            using (BinaryReader binr = new BinaryReader(new MemoryStream(publickeyBits)))
            {
                ushort twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)    
                {
                    binr.ReadByte();    //advance 1 byte  
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();   //advance 2 bytes    
                }
                else
                {
                    throw new Exception("Unexpected value read binr.ReadUInt16() 0x8130 or 0x8230");
                }

                // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"    
                byte[] seqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
                byte[] seq = binr.ReadBytes(15);       //read the Sequence OID    
                if (!CompareBytearrays(seq, seqOID)) throw new Exception("make sure Sequence for OID is correct");

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)    
                {
                    binr.ReadByte();    //advance 1 byte   
                }
                else if (twobytes == 0x8203)
                {
                    binr.ReadInt16();   //advance 2 bytes    
                }
                else
                {
                    throw new Exception("Unexpected value read binr.ReadUInt16() 0x8103 or 0x8203");
                }

                byte bt = binr.ReadByte();
                if (bt != 0x00) throw new Exception("expect null byte next");

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)    
                {
                    binr.ReadByte();    //advance 1 byte   
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();   //advance 2 bytes    
                }
                else
                {
                    throw new Exception("Unexpected value read binr.ReadUInt16() 0x8130 or 0x8230");
                }

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)    
                {
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus    
                }
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte(); //advance 2 bytes    
                    lowbyte = binr.ReadByte();
                }
                else
                {
                    throw new Exception("Unexpected value read binr.ReadUInt16() 0x8102 or 0x8202");
                }

                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order    
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {
                    //if first byte (highest order) of modulus is zero, don't include it    
                    binr.ReadByte();    //skip this null byte    
                    modsize -= 1;   //reduce modulus buffer size by 1    
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes    

                if (binr.ReadByte() != 0x02) throw new Exception("expect an Integer for the exponent data");

                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)    
                byte[] exponent = binr.ReadBytes(expbytes);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----    
                var rsaProvider = new RSACryptoServiceProvider();
                var rsaParams = new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = exponent
                };

                rsaProvider.ImportParameters(rsaParams);

                return rsaProvider;
            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int i = 0;

            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }

                i++;
            }

            return true;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            int count;

            byte bt = binr.ReadByte();

            if (bt != 0x02)
            {
                return 0;
            }

            bt = binr.ReadByte();

            if (bt == 0x81)
            {
                count = binr.ReadByte();
            }
            else if (bt == 0x82)
            {
                byte highbyte = binr.ReadByte();
                byte lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }

            binr.BaseStream.Seek(-1, SeekOrigin.Current);

            return count;
        }

        #endregion

        #region DES

        /// <summary>
        /// DES加密（对称）
        /// </summary>
        /// <param name="content">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string DES加密(string content, string key)
        {
            byte[] encryptedArray = Encoding.UTF8.GetBytes(content);

            var md5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = md5CryptoService.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5CryptoService.Clear();

            var tripleDESCryptoService = new TripleDESCryptoServiceProvider
            {
                Key = securityKeyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var crytpoTransform = tripleDESCryptoService.CreateEncryptor();

            byte[] resultArray = crytpoTransform.TransformFinalBlock(encryptedArray, 0, encryptedArray.Length);

            tripleDESCryptoService.Clear();

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// DES解密（对称）
        /// </summary>
        /// <param name="content">密文</param>
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string DES解密(string content, string key)
        {
            byte[] decryptArray = Convert.FromBase64String(content);

            var md5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = md5CryptoService.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5CryptoService.Clear();

            var tripleDESCryptoService = new TripleDESCryptoServiceProvider
            {
                Key = securityKeyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var crytpoTransform = tripleDESCryptoService.CreateDecryptor();

            byte[] resultArray = crytpoTransform.TransformFinalBlock(decryptArray, 0, decryptArray.Length);

            tripleDESCryptoService.Clear();

            return Encoding.UTF8.GetString(resultArray);
        }

        #endregion

        #region AES

        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="content">明文（待加密）</param>
        /// <param name="key">密文</param>
        public static string AES加密(string content, string key)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            byte[] toEncryptArray = Encoding.UTF8.GetBytes(content);

            using (var managed = new RijndaelManaged())
            {
                managed.Key = Encoding.UTF8.GetBytes(key);
                managed.Mode = CipherMode.ECB;
                managed.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = managed.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="content">明文（待解密）</param>
        /// <param name="key">密文</param>
        public static string AES解密(string content, string key)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            byte[] toEncryptArray = Convert.FromBase64String(content);

            using (var managed = new RijndaelManaged())
            {
                managed.Key = Encoding.UTF8.GetBytes(key);
                managed.Mode = CipherMode.ECB;
                managed.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = managed.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
        }

        #endregion
    }
}