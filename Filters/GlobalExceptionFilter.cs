using Easy.Common.NetCore.Exceptions;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.UI;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Easy.Common.NetCore.Filters
{
    /// <summary>
    /// 异常处理
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly IConfiguration _configuration = EasyIocContainer.Container.GetInstance<IConfiguration>();

        public void OnException(ExceptionContext executedContext)
        {
            executedContext.ExceptionHandled = true;
            bool isAjaxRequest = executedContext.HttpContext.Request.IsAjaxRequest();

            if (isAjaxRequest && executedContext.HttpContext.Request.Query["NeedLayout"] == "false")
            {
                //如果是不需要母版页的ajax请求获取页面Html内容，不做处理，让ajax的error function()来处理
                return;
            }

            SysApiResult<string> result;

            if (executedContext.Exception is FException)
            {
                result = new SysApiResult<string>() { Status = SysApiStatus.失败, Message = executedContext.Exception.Message };
            }
            else if (executedContext.Exception is AntiforgeryValidationException)
            {
                result = new SysApiResult<string>() { Status = SysApiStatus.拦截, Message = "服务器繁忙，请重新登陆。" };
            }
            else
            {
                logger.Error(executedContext.Exception, "全局异常捕获");
                result = new SysApiResult<string>() { Status = SysApiStatus.异常, Message = "服务器繁忙，请稍候再试" };
            }

            if (isAjaxRequest)
            {
                executedContext.Result = new JsonNetResult(value: result);
            }
            else
            {
                string errorRedirect = _configuration?["appSettings:ErrorRedirect"];

                if (!string.IsNullOrWhiteSpace(errorRedirect))
                {
                    if (executedContext.Exception is FException)
                    {
                        errorRedirect = $"{errorRedirect}?message={executedContext.Exception.Message}";
                    }

                    executedContext.Result = new RedirectResult(errorRedirect);
                }
                else
                {
                    executedContext.Result = new JsonNetResult(value: result);
                }
            }
        }
    }
}