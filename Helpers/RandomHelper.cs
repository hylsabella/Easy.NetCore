using System;
using System.Security.Cryptography;
using System.Text;

namespace Easy.Common.NetCore.Helpers
{
    public static class RandomHelper
    {
        /// <summary>
        /// 生成【指定位数】的十六进制字符随机序列
        /// </summary>
        public static string GetHexSequence(int length)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            byte[] buffer = new byte[length / 2];

            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(buffer);
            }

            StringBuilder builder = new StringBuilder(length);

            for (int i = 0; i < buffer.Length; i++)
            {
                builder.Append(string.Format("{0:X2}", buffer[i]));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="maxValue">随机数的最大值（最小值是 0）</param>
        /// <returns>随机数</returns>
        public static int GetRandom(int maxValue)
        {
            if (maxValue <= 0)
            {
                return 0;
            }

            maxValue++;

            byte[] bytes = new byte[4];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            uint randomResult = BitConverter.ToUInt32(bytes, 0);

            return (int)(randomResult % maxValue);
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="minValue">随机数的最小值（最小值是 0）</param>
        /// <param name="maxValue">随机数的最大值（最小值是 0）</param>
        /// <returns>随机数</returns>
        public static int GetRandom(int minValue, int maxValue)
        {
            if (minValue < 0)
            {
                minValue = 0;
            }

            if (maxValue < 0)
            {
                maxValue = 0;
            }

            if (minValue == maxValue)
            {
                return minValue;
            }

            if (minValue > maxValue)
            {
                return 0;
            }

            int result = GetRandom(maxValue - minValue) + minValue;

            return result;
        }
    }
}