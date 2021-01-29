using System;

namespace Easy.Common.NetCore.Helpers
{
    public static class IdentityCardHelper
    {
        /// <summary>
        /// 验证身份证
        /// </summary>
        public static bool CheckIdentityCard(string identityCard)
        {
            if (string.IsNullOrWhiteSpace(identityCard)) return false;

            if (identityCard.Length == 18)
            {
                return CheckIdentityCard18(identityCard);
            }
            else if (identityCard.Length == 15)
            {
                return CheckIdentityCard15(identityCard);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 验证18位身份证 
        /// </summary>
        private static bool CheckIdentityCard18(string identityCard)
        {
            if (long.TryParse(identityCard.Remove(17), out long n) == false ||
                n < Math.Pow(10, 16) ||
                long.TryParse(identityCard.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(identityCard.Remove(2)) == -1)
            {
                return false;
            }

            string birth = identityCard.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            if (DateTime.TryParse(birth, out DateTime time) == false)
            {
                return false;
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');

            char[] Ai = identityCard.Remove(17).ToCharArray();

            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != identityCard.Substring(17, 1).ToLower())
            {
                return false;
            }

            return true;//正确
        }

        /// <summary>
        /// 验证15位身份证 
        /// </summary>
        private static bool CheckIdentityCard15(string identityCard)
        {
            if (long.TryParse(identityCard, out long n) == false || n < Math.Pow(10, 14))
            {
                return false;
            }

            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(identityCard.Remove(2)) == -1)
            {
                return false;
            }

            string birth = identityCard.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            if (DateTime.TryParse(birth, out DateTime time) == false)
            {
                return false;
            }

            return true;//正确
        }
    }
}