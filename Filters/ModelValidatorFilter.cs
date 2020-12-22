using Easy.Common.NetCore.UI;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Easy.Common.NetCore.Filters
{
    /// <summary>
    /// 模型数据验证
    /// </summary>
    public class ModelValidatorFilter : IActionFilter
    {
        /// <summary>
        /// 在操作执行之后，在操作结果之前调用。
        /// </summary>
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        /// <summary>
        /// 在操作执行之前、模型绑定完成后调用。
        /// </summary>
        public void OnActionExecuting(ActionExecutingContext actionContext)
        {
            if (actionContext.ActionArguments.Count > 0)
            {
                var model = actionContext.ActionArguments.First().Value;

                if (model == null)
                {
                    var result = new SysApiResult<string>() { Status = SysApiStatus.未授权, Message = "请求参数不能为空！" };
                    actionContext.Result = new JsonNetResult(value: result);

                    return;
                }
            }

            if (!actionContext.ModelState.IsValid)
            {
                ModelError firstError = new ModelError("未知错误");

                for (int i = 0; i < actionContext.ModelState.Keys.Count(); i++)
                {
                    var errorList = actionContext.ModelState.Values.ElementAt(i).Errors;

                    if (errorList == null || errorList.Count <= 0)
                    {
                        continue;
                    }

                    firstError = errorList.First();

                    break;
                }

                string errorMsg = string.IsNullOrWhiteSpace(firstError.ErrorMessage) ?
                    firstError.Exception?.Message ?? "" :
                    firstError.ErrorMessage;

                var result = new SysApiResult<string>() { Status = SysApiStatus.异常, Message = errorMsg };
                actionContext.Result = new JsonNetResult(value: result);

                return;
            }
        }
    }
}