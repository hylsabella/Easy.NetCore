using Easy.Common.NetCore.Extentions;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Easy.Common.NetCore.Controllers
{
    public class ApiController : ControllerBase
    {
        private static readonly ITokenSvc _tokenSvc = EasyIocContainer.GetInstance<ITokenSvc>();

        public virtual UserIdentity CurrentUser
        {
            get
            {
                if (!HttpContext.Request.TryGetHeader("accessToken", out string accessToken))
                {
                    return null;
                }

                var tokenModel = _tokenSvc.DecodeToken(accessToken);

                if (tokenModel == null)
                {
                    return null;
                }

                return new UserIdentity
                {
                    Token = accessToken,
                    UserId = tokenModel.UserId,
                    UserName = tokenModel.UserName,
                    TokenExpireTime = tokenModel.TokenExpireTime,
                };
            }
        }

        public virtual JsonResult Json(object data)
        {
            return new JsonNetResult(data);
        }

        public virtual ModelError ModelFirstError
        {
            get
            {
                ModelError firstError = null;

                for (int i = 0; i < this.ModelState.Keys.Count(); i++)
                {
                    var errorList = this.ModelState.Values.ElementAt(i).Errors;

                    if (errorList == null || errorList.Count <= 0)
                    {
                        continue;
                    }

                    firstError = errorList.First();

                    break;
                }

                return firstError;
            }
        }
    }
}