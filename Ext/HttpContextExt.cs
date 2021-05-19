using Microsoft.AspNetCore.Http;
using NLog;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Easy.Common.NetCore
{
    public static class HttpContextExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool TryGetHeader(this HttpContext httpContext, string headerName, out string value)
        {
            value = string.Empty;

            if (!httpContext.Request.Headers.Keys.Contains(headerName))
            {
                return false;
            }

            value = httpContext.Request.Headers[headerName];

            return true;
        }

        /// <summary>
        /// 获取访问nginx服务器的客服机真实IP（可能是真实客户机IP，也可能是代理IP）
        /// 注：该方法是用于部署了nginx反向代理服务器的场景
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetIPOfCallNginx(this HttpContext httpContext)
        {
            /*
                                                         客户机3
                                                            -
                                                            -
                客户机1               客户机2              代理1
                      -                 -               -
                          -             -           -
                              -         -       -
                                   nginx服务器
                              -         -       -
                          -             -           -
                      -                 -               -
                应用服务器1          应用服务器2          应用服务器3
             */

            // 如上图所示：只能获得【客户机1】、【客户机2】、【代理1】的真实IP，该IP是不存在伪造的IP
            // 如果【客户机3】攻击服务器，那么，直接把【客户机3】用的【代理1】封掉
            //【X-Real-IP】是直接访问nginx服务器的真实IP，由【nginx】传递过来

            return httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }

        /// <summary>  
        /// 获取IP（想获取到真实是IP，前提条件是不存在伪造，如果第一台客户机伪造了【X-Forwarded-For】，那么服务器也拿不到真实的IP）
        /// </summary>  
        /// <returns></returns>  
        public static string GetRealIP(this HttpContext httpContext)
        {
            //TODO: 检测【X-Forwarded-For】信息是否会引发【SQL注入】或【XSS】安全漏洞

            /*获取IP的步骤（不管是否有【nginx】反向代理服务器）
                1.获取【X-Real-IP】地址。如果有，说明是访问【nginx】的真实IP；如果没有，说明没有使用【nginx】，继续下一步
                2.获取【X-Forwarded-For】地址，如果有，说明使用了代理，则拿到第一个IP；如果没有，说明没有使用代理，继续下一步
                3.获取【UserHostAddress】地址，该地址与【REMOTE_ADDR】一致，是真实IP
            */

            try
            {
                //1.获取【X-Real-IP】地址
                //如果部署了【nginx】服务器，那么就获取直接访问【nginx】服务器的客户机（有可能是代理）
                string resultIp = httpContext.GetIPOfCallNginx();

                if (!string.IsNullOrWhiteSpace(resultIp))
                {
                    return resultIp;
                }

                //2.获取【X-Forwarded-For】地址
                //resultIp = request.ServerVariables["X-Forwarded-For"];
                resultIp = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                //可能有代理   
                if (!string.IsNullOrWhiteSpace(resultIp))
                {
                    //没有"." 肯定是非IP格式  
                    if (!resultIp.Contains("."))
                    {
                        resultIp = string.Empty;
                    }
                    else
                    {
                        //有","表示经过了多个代理。取第一个不是内网的IP。  
                        if (resultIp.Contains(","))
                        {
                            resultIp = resultIp.Replace(" ", string.Empty)
                                           .Replace(";", string.Empty)
                                           .Replace("\"", string.Empty);

                            string[] proxyIps = resultIp.Split(",".ToCharArray()) ?? new string[] { };

                            foreach (var ip in proxyIps)
                            {
                                //是IP格式，并且不是内网地址
                                if (IsIPAddress(ip) &&
                                   ip.Substring(0, 3) != "10." &&
                                   ip.Substring(0, 7) != "192.168" &&
                                   ip.Substring(0, 7) != "172.16." &&
                                   ip.Substring(0, 7) != "172.31.")
                                {
                                    return ip;
                                }
                            }
                        }
                        //是IP格式  
                        else if (IsIPAddress(resultIp))
                        {
                            return resultIp;
                        }
                        //内容非IP  
                        else
                        {
                            resultIp = string.Empty;
                        }
                    }
                }

                //没有代理，则直接拿REMOTE_ADDR，这是真实IP
                if (string.IsNullOrWhiteSpace(resultIp))
                {
                    resultIp = httpContext.Connection.RemoteIpAddress.ToString();
                }

                return resultIp;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "获取客户IP出现异常");

                return string.Empty;
            }
        }

        public static bool IsIPAddress(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || str.Length < 7 || str.Length > 15)
                return false;

            string regformat = @"^(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})";
            Regex regex = new Regex(regformat, RegexOptions.IgnoreCase);

            return regex.IsMatch(str);
        }
    }
}