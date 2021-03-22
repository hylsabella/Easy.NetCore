using System;
using System.IO;

namespace Easy.Common.NetCore.Helpers
{
    public static class IPHelper
    {
        public static string GetCityByIP(string ip)
        {
            string position = string.Empty;

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
                        position += ipList[2] + " ";
                    }

                    if (ipList.Length >= 4)
                    {
                        position += ipList[3] + " ";
                    }

                    if (ipList.Length >= 5)
                    {
                        position += ipList[4] + " ";
                    }

                    if (ipList.Length >= 6)
                    {
                        position += ipList[5] + " ";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, "GetCityByIP出错");
            }

            return position.Trim();
        }
    }
}