using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace Easy.Common.NetCore.Security
{
    /// <summary>
    /// Cookie验证服务
    /// </summary>
    public class CookieAuthSvc : ICookieAuthSvc
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="userIdentity">UserIdentity</param>
        /// <param name="isPersistent">是否启用过期时间
        /// （即便用户关闭了浏览器，过期时间内再次访问站点仍然处于登录状态，除非调用Logout方法注销登录。）</param>
        /// <param name="expiresUtc">Cookie过期时间（设置 ExpiresUtc 后，它将覆盖 CookieAuthenticationOptions的 ExpireTimeSpan 选项的值）</param>
        public virtual void SignIn(HttpContext httpContext, UserIdentity userIdentity, bool isPersistent = true, DateTimeOffset? expiresUtc = null)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext不能为空！");
            if (userIdentity == null) throw new ArgumentNullException("userIdentity不能为空！");

            var claims = new[] { new Claim("UserIdentity", JsonConvert.SerializeObject(userIdentity)) };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = isPersistent,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true //是否允许刷新
            };

            httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties).Wait();
        }

        /// <summary>
        /// 登出
        /// </summary>
        public virtual void SignOut(HttpContext httpContext)
        {
            if (httpContext != null)
            {
                httpContext.SignOutAsync().Wait();
            }
        }
    }
}