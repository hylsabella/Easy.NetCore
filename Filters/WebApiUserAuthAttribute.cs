using Easy.Common.NetCore.Enums;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.Security;
using Easy.Common.NetCore.UI;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Easy.Common.NetCore.Filters
{
    /// <summary>
    /// 用户权限验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class WebApiUserAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string _userTokenKey = "accessToken";
        private static readonly ITokenSvc _tokenSvc = EasyIocContainer.GetInstance<ITokenSvc>();

        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null) throw new ArgumentNullException("filterContext");

            if (!filterContext.HttpContext.Request.TryGetHeader(_userTokenKey, out string accessToken))
            {
                var result = new SysApiResult<string>() { Status = SysApiStatus.未授权, Message = "您的登陆身份已过期，请重新登陆" };

                filterContext.Result = new JsonNetResult(value: result);

                return;
            }

            var tokenModel = _tokenSvc.DecodeToken(accessToken);

            //从数据库获取客户信息
            if (tokenModel == null)
            {
                var result = new SysApiResult<string>() { Status = SysApiStatus.未授权, Message = "您的身份未授权" };

                filterContext.Result = new JsonNetResult(value: result);

                return;
            }

            //时间过期
            if (tokenModel.TokenExpireTime <= DateTime.Now)
            {
                var result = new SysApiResult<string>() { Status = SysApiStatus.未授权, Message = "token已过期，请重新登陆" };

                filterContext.Result = new JsonNetResult(value: result);

                return;
            }

            if (!filterContext.HttpContext.Request.TryGetHeader("DeviceType", out string deviceTypeStr) ||
                !Enum.TryParse(deviceTypeStr, out DeviceType deviceType))
            {
                var result = new SysApiResult<string>() { Status = SysApiStatus.异常, Message = "缺失DeviceType" };

                filterContext.Result = new JsonNetResult(value: result);

                return;
            }

            bool isOk = _tokenSvc.检查用户登陆是否合法(tokenModel.UserId, deviceType, accessToken, isAdmin: false, isSingleLogin: true, out string errorMsg);

            if (!isOk)
            {
                var result = new SysApiResult<string>() { Status = SysApiStatus.未授权, Message = errorMsg };

                filterContext.Result = new JsonNetResult(value: result);

                return;
            }
        }
    }
}