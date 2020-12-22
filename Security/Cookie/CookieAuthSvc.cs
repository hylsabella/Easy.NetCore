using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace Easy.Common.NetCore.Security
{
    /// <summary>
    /// Cookie��֤����
    /// </summary>
    public class CookieAuthSvc : ICookieAuthSvc
    {
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="userIdentity">UserIdentity</param>
        /// <param name="isPersistent">�Ƿ����ù���ʱ��
        /// �������û��ر��������������ʱ�����ٴη���վ����Ȼ���ڵ�¼״̬�����ǵ���Logout����ע����¼����</param>
        /// <param name="expiresUtc">Cookie����ʱ�䣨���� ExpiresUtc ���������� CookieAuthenticationOptions�� ExpireTimeSpan ѡ���ֵ��</param>
        public virtual void SignIn(HttpContext httpContext, UserIdentity userIdentity, bool isPersistent = true, DateTimeOffset? expiresUtc = null)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext����Ϊ�գ�");
            if (userIdentity == null) throw new ArgumentNullException("userIdentity����Ϊ�գ�");

            var claims = new[] { new Claim("UserIdentity", JsonConvert.SerializeObject(userIdentity)) };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = isPersistent,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true //�Ƿ�����ˢ��
            };

            httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties).Wait();
        }

        /// <summary>
        /// �ǳ�
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