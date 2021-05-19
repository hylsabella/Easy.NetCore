using System;

namespace Easy.Common.NetCore
{
    public static class DateTimeExt
    {
        public static long GetTimeStampBySecond(this DateTime target)
        {
            DateTimeOffset dto = new DateTimeOffset(target);

            var result = dto.ToUnixTimeSeconds();

            return result;
        }

        public static DateTime GetDateTimeBySecondTimeStamp(this long timeStamp)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(timeStamp);

            var result = dto.ToLocalTime().DateTime;

            return result;
        }

        public static long GetTimeStampByMiSecond(this DateTime target)
        {
            DateTimeOffset dto = new DateTimeOffset(target);

            var result = dto.ToUnixTimeMilliseconds();

            return result;
        }

        public static DateTime GetDateTimeByMiSecondTimeStamp(this long timeStamp)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(timeStamp);

            var result = dto.ToLocalTime().DateTime;

            return result;
        }

        /// <summary>
        /// MM-dd hh:mm:ss
        /// </summary>
        public static string ToShortTime(this DateTime dt)
        {
            return dt.ToString("MM-dd HH:mm:ss");
        }

        /// <summary>
        /// MM-dd hh:mm:ss
        /// </summary>
        public static string ToShortTime(this DateTime? dt)
        {
            if (dt == null)
            {
                return string.Empty;
            }

            return dt.Value.ToString("MM-dd HH:mm:ss");
        }

        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string ToLongTime(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string ToLongTime(this DateTime? dt)
        {
            if (dt == null)
            {
                return string.Empty;
            }

            return dt.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        public static string ToDate(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        public static string ToDate(this DateTime? dt)
        {
            if (dt == null)
            {
                return string.Empty;
            }

            return dt.Value.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 获取制定的时间距离当前时间的距离
        /// </summary>
        public static string GetDistanceAtNow(this DateTime dt)
        {
            string result = string.Empty;
            //为了避免显示太长，只显示2个时间类型的数据
            int timeCount = 0;

            TimeSpan ts = (DateTime.Now - dt).Duration();

            if (ts.Days > 0 && timeCount < 2)
            {
                result += $"{ts.Days}天";
                timeCount++;
            }
            if (ts.Hours > 0 && timeCount < 2)
            {
                result += $"{ts.Hours}小时";
                timeCount++;
            }
            if (ts.Minutes > 0 && timeCount < 2)
            {
                result += $"{ts.Minutes}分钟";
                timeCount++;
            }
            if (ts.Seconds > 0 && timeCount < 2)
            {
                result += $"{ts.Seconds}秒";
            }

            if (DateTime.Now > dt)
            {
                result += "前";
            }
            else if (dt > DateTime.Now)
            {
                result += "后";
            }
            else
            {
                result = "刚刚";
            }

            return result;
        }
    }
}