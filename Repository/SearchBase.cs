using System;

namespace Easy.Common.NetCore.Repository
{
    public class SearchBase
    {
        /// <summary>
        /// 开始时间（包括该时间点）
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间（不包括该时间点）
        /// </summary>
        public DateTime? EndTime { get; set; }

        public string TableIndex { get; set; }

        /// <summary>
        /// 开始索引
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// 结束索引（数据库SQL使用）
        /// </summary>
        public int EndIndex
        {
            get
            {
                if (PageCount <= 0)
                {
                    PageCount = int.MaxValue;
                }

                return StartIndex + PageCount;
            }
        }

        /// <summary>
        /// 每页条目数
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public string OrderBy { get; set; }

        public void GetOrderByParam(out string orderByName, out string orderByAscDesc)
        {
            orderByName = string.Empty;
            orderByAscDesc = string.Empty;

            if (string.IsNullOrWhiteSpace(this.OrderBy))
            {
                return;
            }

            var splits = OrderBy.Split(new string[] { "-" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length != 2)
            {
                return;
            }

            orderByName = splits[0]?.Trim();
            orderByAscDesc = splits[1];
        }

        public virtual string GetOrderBy()
        {
            this.GetOrderByParam(out string orderByName, out string orderByAscDesc);

            if (!string.IsNullOrWhiteSpace(orderByName))
            {
                return $"{orderByName} {orderByAscDesc}";
            }
            else
            {
                return " Id DESC ";
            }
        }
    }
}