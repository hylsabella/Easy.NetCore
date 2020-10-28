using System.Collections.Generic;

namespace Easy.Common.NetCore.UI
{
    public class PageUIResult<T>
    {
        public int draw { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int recordsTotal { get; set; } = 0;

        public int recordsFiltered { get; set; } = 0;

        /// <summary>
        /// 当页记录集合
        /// </summary>
        public IList<T> data { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string error { get; set; }

        /// <summary>
        /// 构建错误的结果 error中不要含有"-"
        /// </summary>
        public PageUIResult<T> DefaultErrorResult(int draw, string error = "系统繁忙，请稍后再试")
        {
            this.draw = draw;
            this.error = error;
            this.data = new List<T>();
            this.recordsTotal = 0;
            this.recordsFiltered = 0;

            return this;
        }

        /// <summary>
        /// 默认的空记录处理
        /// </summary>
        public PageUIResult<T> DefaultNullResult(int draw)
        {
            this.draw = draw;
            this.data = new List<T>();
            this.recordsTotal = 0;
            this.recordsFiltered = 0;

            return this;
        }
    }
}