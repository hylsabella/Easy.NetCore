using System.Collections.Generic;

namespace Easy.Common.NetCore.Repository
{
    public class PageResult<T>
    {
        public int TotalCount { get; set; }

        public IList<T> Results { get; set; }

        public int PageCount
        {
            get
            {
                return Results == null ? 0 : Results.Count;
            }
        }
    }
}