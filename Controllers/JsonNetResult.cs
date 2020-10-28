using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Easy.Common.NetCore
{
    public class JsonNetResult : JsonResult
    {
        public JsonNetResult(object value) : base(value) { }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null) throw new ArgumentException("context不能为null");

            context.HttpContext.Response.ContentType = string.IsNullOrEmpty(ContentType) ? "application/json" : ContentType;

            var serializerSettings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            return context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(Value, serializerSettings));
        }
    }
}