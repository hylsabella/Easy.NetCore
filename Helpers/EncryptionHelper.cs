using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Easy.Common.NetCore.Helpers
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// DES加密（对称）
        /// </summary>
        /// <param name="content">明文</param>
        /// <param name="sKey">密钥</param>
        /// <returns>密文</returns>
        public static string DES加密(string content, string sKey)
        {
            //将要加密的内容转换成一个Byte数组
            byte[] inputByteArray = Encoding.Default.GetBytes(content);

            //创建一个DES加密服务提供者
            var des = new DESCryptoServiceProvider
            {
                //设置密钥和初始化向量
                Key = Encoding.ASCII.GetBytes(sKey),
                IV = Encoding.ASCII.GetBytes(sKey)
            };

            //创建一个内存流对象
            var ms = new MemoryStream();

            //创建一个加密流对象
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);

            //将要加密的文本写到加密流中
            cs.Write(inputByteArray, 0, inputByteArray.Length);

            //更新缓冲
            cs.FlushFinalBlock();

            //获取加密过的文本
            var sb = new StringBuilder();

            foreach (byte b in ms.ToArray())
            {
                sb.AppendFormat("{0:X2}", b);
            }

            //释放资源
            cs.Close();
            ms.Close();
            des.Clear();

            return sb.ToString();
        }

        /// <summary>
        /// DES解密（对称）
        /// </summary>
        /// <param name="sContent">密文</param>
        /// <param name="sKey">密钥</param>
        /// <returns>明文</returns>
        public static string DES解密(string sContent, string sKey)
        {
            /* 将要解密的内容转换成一个Byte数组 */
            byte[] inputByteArray = new byte[sContent.Length / 2];

            for (int x = 0; x < sContent.Length / 2; x++)
            {
                int i = (Convert.ToInt32(sContent.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            //创建一个DES加密服务提供者
            var des = new DESCryptoServiceProvider
            {
                //设置密钥和初始化向量
                Key = Encoding.ASCII.GetBytes(sKey),
                IV = Encoding.ASCII.GetBytes(sKey)
            };

            //创建一个内存流对象
            var ms = new MemoryStream();

            //创建一个加密流对象
            using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
            {
                //将要解密的文本写到加密流中
                cs.Write(inputByteArray, 0, inputByteArray.Length);

                //更新缓冲
                cs.FlushFinalBlock();

                string result = Encoding.Default.GetString(ms.ToArray());

                //释放资源
                ms.Close();
                des.Clear();

                return result;
            }
        }
    }
}