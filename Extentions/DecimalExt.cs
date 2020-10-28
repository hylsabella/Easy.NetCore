using System;

namespace Easy.Common.NetCore.Extentions
{
    public static class DecimalExt
    {
        public static decimal FormatN(this decimal value, uint count = 0)
        {
            //整数部分
            decimal intValue = decimal.Truncate(value);

            //小数部分
            decimal pointValue = value - intValue;

            if (count <= 0)
            {
                return intValue;
            }

            decimal digit = Convert.ToDecimal(Math.Pow(10, count));

            decimal pointDigit = decimal.Truncate(pointValue * digit);

            return intValue + (pointDigit / digit);
        }

        public static string FormatNStr(this decimal value, uint count = 0)
        {
            return FormatN(value, count).ToString();
        }
    }
}