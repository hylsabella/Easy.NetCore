using Easy.Common.NetCore.Security;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Easy.Common.NetCore.UI
{
    /// <summary>
    /// 视图渲染
    /// </summary>
    public abstract class EasyRazorPage<TModel> : RazorPage<TModel>
    {
        /// <summary>
        /// 设置母版页页（ajax请求不需要重复加载母版页）
        /// </summary>
        public bool IsNeedLayout()
        {
            if (this.Context.Request.Query["NeedLayout"] == "false")
            {
                Layout = "";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 当前用户信息
        /// </summary>
        public UserIdentity CurrentUser
        {
            get
            {
                if (!this.User.Identity.IsAuthenticated ||
                    !this.User.HasClaim(x => x.Type == "UserIdentity"))
                {
                    return new UserIdentity { UserName = "请先登录" };
                }

                var userJson = this.User.FindFirstValue("UserIdentity");

                var userModel = JsonConvert.DeserializeObject<UserIdentity>(userJson);

                return userModel;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return this?.User?.Identity?.IsAuthenticated ?? false;
            }
        }
    }
}