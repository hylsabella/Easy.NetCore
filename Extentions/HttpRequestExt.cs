using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace Easy.Common.NetCore.Extentions
{
    public static class HttpRequestExt
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            if (request.Headers != null)
            {
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }

            return false;
        }

        public static string GetRealIP(this HttpRequest httpRequest)
        {
            return httpRequest.HttpContext.GetRealIP();
        }

        public static bool TryGetHeader(this HttpRequest httpRequest, string headerName, out string value)
        {
            value = string.Empty;

            if (!httpRequest.Headers.TryGetValue(headerName, out StringValues headers))
            {
                return false;
            }

            value = headers.FirstOrDefault() ?? string.Empty;

            return true;
        }
    }
}
