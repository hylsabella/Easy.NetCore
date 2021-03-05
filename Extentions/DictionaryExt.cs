using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easy.Common.NetCore.Extentions
{
    public static class DictionaryExt
    {
        public static string GetUrlParams(this Dictionary<string, string> dict)
        {
            if (dict == null || !dict.Any())
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var item in dict)
            {
                sb.Append($"{item.Key}={item.Value}&");
            }

            return sb.ToString().TrimEnd('&');
        }
    }
}