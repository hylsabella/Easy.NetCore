using Microsoft.AspNetCore.Http;
using System;

namespace Easy.Common.NetCore.Security
{
    /// <summary>
    /// Cookie验证服务
    /// </summary>
    public interface ICookieAuthSvc
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="user">UserIdentity</param>
        /// <param name="isPersistent">是否启用过期时间</param>
        /// <param name="expiresUtc">Cookie过期时间（设置 ExpiresUtc 后，它将覆盖 CookieAuthenticationOptions的 ExpireTimeSpan 选项的值）</param>
        void SignIn(HttpContext httpContext, UserIdentity user, bool isPersistent = true, DateTimeOffset? expiresUtc = null);

        /// <summary>
        /// 登出
        /// </summary>
        void SignOut(HttpContext httpContext);
    }
}