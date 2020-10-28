using Easy.Common.NetCore.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;

namespace Easy.Common.NetCore.Controllers
{
    public class MvcController : Controller
    {
        public virtual UserIdentity CurrentUser
        {
            get
            {
                if (!this.User.Identity.IsAuthenticated ||
                    !this.User.HasClaim(x => x.Type == "UserIdentity"))
                {
                    return null;
                }

                var userJson = this.User.FindFirstValue("UserIdentity");
                var userModel = JsonConvert.DeserializeObject<UserIdentity>(userJson);

                return userModel;
            }
        }

        public override JsonResult Json(object data)
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