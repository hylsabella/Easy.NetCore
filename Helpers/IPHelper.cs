using System;
using System.IO;

namespace Easy.Common.NetCore.Helpers
{
    public static class IPHelper
    {
        public static (string province, string city) GetCityByIP(string ip)
        {
            string province = string.Empty;
            string city = string.Empty;

            try
            {
                string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/qqzeng-ip-utf8.dat");

                var search = new IPSearch(dataPath);
                string result = search.Query(ip);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var ipList = result.Split('|');

                    if (ipList.Length >= 3)
                    {
                        province = ipList[2];
                    }

                    if (ipList.Length >= 4)
                    {
                        city = ipList[3];
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "GetCityByIP出错");
            }

            return (province, city);
        }
    }
}