using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace Easy.Common.NetCore.Filters
{
    /// <summary>
    /// 用户权限验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class MvcUserAuthAttribute : Attribute, IAuthorizationFilter
    {
        private static readonly IConfiguration _configuration = EasyIocContainer.Container.GetInstance<IConfiguration>();

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            var httpContext = filterContext.HttpContext;
            if (httpContext == null) throw new ArgumentNullException("httpContext");

            bool isAuthenticated = httpContext.User.Identity.IsAuthenticated && (httpContext.User is ClaimsPrincipal);

            if (!isAuthenticated)
            {
                RegirectToLoginUrl(filterContext);
            }
        }

        /// <summary>
        /// 重新跳到登陆页面
        /// </summary>
        private void RegirectToLoginUrl(AuthorizationFilterContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var result = new SysApiResult<string>() { Status = SysApiStatus.未授权, Message = "您的登陆身份已过期，请重新登陆" };

                filterContext.Result = new JsonNetResult(value: result);

                return;
            }

            string loginUrl = _configuration?["appSettings:LoginPath"] ?? "/";

            filterContext.Result = new RedirectResult(loginUrl);
        }
    }
}