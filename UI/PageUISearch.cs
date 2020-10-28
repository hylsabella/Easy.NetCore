using System;

namespace Easy.Common.NetCore.UI
{
    public class PageUISearch
    {
        public int draw { get; set; }

        /// <summary>
        /// 开始位置
        /// </summary>
        public int start { get; set; } = 0;

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int length { get; set; } = 30;

        private DateTime? _beginTime;
        /// <summary>
        /// 开始时间（包括该时间点）
        /// </summary>
        public DateTime? BeginTime
        {
            get
            {
                if (_beginTime != null || string.IsNullOrWhiteSpace(MultiDatePicker))
                {
                    return _beginTime;
                }

                var splits = MultiDatePicker.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

                if (splits.Length != 2)
                {
                    return _beginTime;
                }
                else
                {
                    string begin = splits[0];

                    if (DateTime.TryParse(begin, out DateTime result))
                    {
                        return result;
                    }
                    else
                    {
                        return _beginTime;
                    }
                }
            }
            set
            {
                _beginTime = value;
            }
        }

        private DateTime? _endTime;
        /// <summary>
        /// 结束时间（不包括该时间点）
        /// </summary>
        public DateTime? EndTime
        {
            get
            {
                if (_endTime != null || string.IsNullOrWhiteSpace(MultiDatePicker))
                {
                    return _endTime;
                }

                var splits = MultiDatePicker.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

                if (splits.Length != 2)
                {
                    return _endTime;
                }
                else
                {
                    string end = splits[1];

                    if (DateTime.TryParse(end, out DateTime result))
                    {
                        return result;
                    }
                    else
                    {
                        return _endTime;
                    }
                }
            }
            set
            {
                _endTime = value;
            }
        }

        /// <summary>
        /// 时间范围（"2020-04-22 - 2020-04-28"）
        /// </summary>
        public string MultiDatePicker { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public string OrderBy { get; set; }
    }
}