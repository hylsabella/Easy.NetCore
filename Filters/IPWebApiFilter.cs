using Easy.Common.NetCore.Cache;
using Easy.Common.NetCore.Extentions;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.Security.DefendAttack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace Easy.Common.NetCore.Filters
{
    public class IPWebApiFilter : IAuthorizationFilter
    {
        private static string _freqCount = string.Empty;
        private static readonly IConfiguration _configuration = EasyIocContainer.Container.GetInstance<IConfiguration>();

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                _freqCount = _configuration["appSettings:LimitAttack-MaxCountASecond"] ?? string.Empty;

                //配置为0：表示关闭软防
                if (_freqCount.Trim() == "0")
                {
                    return;
                }

                var request = context.HttpContext.Request;
                var response = context.HttpContext.Response;

                string routeName = request.Path.Value ?? string.Empty;

                //判断是否需要预防该接口
                bool needDefend = DefendAttackContainer.DefendLimitAttackList
                    .Where(x => routeName.Contains(x.Action, StringComparison.OrdinalIgnoreCase))
                    .Any();

                //如果不需要防御，那么就返回不处理
                if (!needDefend)
                {
                    return;
                }

                //检测是否是攻击的请求
                bool hasLimitAttack = CheckHasLimitAttack(request, routeName);

                if (hasLimitAttack)
                {
                    context.Result = new OkResult();

                    //关闭输出
                    request.Body.Close();
                    response.Body.Close();
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 检测是否是攻击的请求
        /// </summary>
        private static bool CheckHasLimitAttack(HttpRequest request, string routeName)
        {
            if (!int.TryParse(_freqCount, out int maxCount))
            {
                //如果没有配置，默认一秒钟最多3次请求
                maxCount = 3;
            }

            //获取IP地址
            string realIP = request.GetRealIP();

            //按具体【RouteName】来预防
            string defendkey = DefendAttackContainer.AssemblyName + routeName;

            IEasyCache easyCache = EasyIocContainer.GetInstance<IEasyCache>();

            //判别是否存在流量攻击
            string key = $"{realIP}:{defendkey}";

            bool hasAttack = easyCache.CheckIsOverStep(key, TimeType.秒, maxCount);

            return hasAttack;
        }
    }
}