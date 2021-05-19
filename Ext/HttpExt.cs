using Easy.Common.NetCore.Exceptions;
using Easy.Common.NetCore.Helpers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easy.Common.NetCore
{
    public static class HttpExt
    {
        public static string HttpPost(this string url, Dictionary<string, object> postParams, HttpPostCfg httpCfg = null)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new FException("url不能为空");
            if (postParams == null || !postParams.Any()) throw new FException("postParams不能为空");

            if (httpCfg == null)
            {
                httpCfg = CreateDefaultHttpPostCfg();
            }
            else if (httpCfg.ReTryCount <= 0)
            {
                httpCfg.ReTryCount = 1;//至少执行一次
            }

            var restRequest = new RestRequest(Method.POST);
            restRequest.Timeout = 10 * 1000;//连接超时设置为10秒

            restRequest.AddHeader("Accept", "*/*");
            string contentType = GetContentType(httpCfg.ContentType);
            restRequest.AddHeader("Content-Type", contentType);

            if (httpCfg.ContentType == ContentType.Json)
            {
                restRequest.AddJsonBody(postParams);
            }
            else if (httpCfg.ContentType == ContentType.Text)
            {
                string queryString = postParams.GetUrlParams();
                restRequest.AddParameter(contentType, queryString, ParameterType.RequestBody);
            }
            else if (httpCfg.ContentType == ContentType.Form)
            {
                foreach (var param in postParams)
                {
                    restRequest.AddParameter(param.Key, param.Value);
                }
            }

            //如果是网络连接异常，那么重试
            IRestResponse restResponse = null;
            CallHelper.ReTryRun(reTryCount: httpCfg.ReTryCount, reTryAction: () =>
            {
                restResponse = new RestClient(url).ExecuteAsync(restRequest).Result;
                bool hasNetWorkError = restResponse.ErrorException is System.Net.WebException;
                bool isReTrySuc = !hasNetWorkError;
                return isReTrySuc;
            }, remark: "HttpPost");

            //响应状态码是否表示成功 OK(200)
            if (!restResponse.IsSuccessful)
            {
                var errSb = new StringBuilder();
                errSb.AppendLine($"{url}接口异常");
                errSb.AppendLine($"Content：{restResponse.Content}");
                errSb.AppendLine($"StatusCode：{restResponse.StatusCode}");
                errSb.AppendLine($"ResponseStatus：{restResponse.ResponseStatus}");
                errSb.AppendLine($"ErrorMessage：{restResponse.ErrorMessage}");

                throw new Exception(errSb.ToString(), restResponse.ErrorException);
            }

            return restResponse.Content;
        }

        public static string HttpGet(this string url, Dictionary<string, object> getParams, HttpGetCfg httpCfg = null)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new FException("url不能为空");
            if (getParams == null || !getParams.Any()) throw new FException("getParams不能为空");

            if (httpCfg == null)
            {
                httpCfg = CreateDefaultHttpGetCfg();
            }
            else if (httpCfg.ReTryCount <= 0)
            {
                httpCfg.ReTryCount = 1;//至少执行一次
            }

            var restRequest = new RestRequest(Method.GET);
            restRequest.Timeout = 10 * 1000;//连接超时设置为10秒
            restRequest.AddHeader("Accept", "*/*");

            if (httpCfg.IsFormContentType)
            {
                string contentType = GetContentType(ContentType.Form);
                restRequest.AddHeader("Content-Type", contentType);
            }

            foreach (var param in getParams)
            {
                restRequest.AddParameter(param.Key, param.Value);
            }

            //如果是网络连接异常，那么重试
            IRestResponse restResponse = null;
            CallHelper.ReTryRun(reTryCount: httpCfg.ReTryCount, reTryAction: () =>
            {
                restResponse = new RestClient(url).ExecuteAsync(restRequest).Result;
                bool hasNetWorkError = restResponse.ErrorException is System.Net.WebException;
                bool isReTrySuc = !hasNetWorkError;
                return isReTrySuc;
            }, remark: "HttpGet");

            //响应状态码是否表示成功 OK(200)
            if (!restResponse.IsSuccessful)
            {
                var errSb = new StringBuilder();
                errSb.AppendLine($"{url}接口异常");
                errSb.AppendLine($"Content：{restResponse.Content}");
                errSb.AppendLine($"StatusCode：{restResponse.StatusCode}");
                errSb.AppendLine($"ResponseStatus：{restResponse.ResponseStatus}");
                errSb.AppendLine($"ErrorMessage：{restResponse.ErrorMessage}");

                throw new Exception(errSb.ToString(), restResponse.ErrorException);
            }

            return restResponse.Content;
        }

        private static HttpPostCfg CreateDefaultHttpPostCfg()
        {
            var httpCfg = new HttpPostCfg
            {
                ReTryCount = 3,
                ContentType = ContentType.Json,
            };

            return httpCfg;
        }

        private static HttpGetCfg CreateDefaultHttpGetCfg()
        {
            var httpCfg = new HttpGetCfg
            {
                ReTryCount = 3,
                IsFormContentType = false,
            };

            return httpCfg;
        }

        private static string GetContentType(ContentType contentType)
        {
            switch (contentType)
            {
                case ContentType.Json:
                    return "application/json";
                case ContentType.Text:
                    return "text/plain";
                case ContentType.Form:
                    return "application/x-www-form-urlencoded";
                default:
                    return string.Empty;
            }
        }
    }

    public class HttpPostCfg
    {
        public uint ReTryCount { get; set; } = 3;

        public ContentType ContentType { get; set; }
    }

    public class HttpGetCfg
    {
        public uint ReTryCount { get; set; } = 3;

        public bool IsFormContentType { get; set; }
    }

    public enum ContentType
    {
        Json = 0,
        Text,
        Form,
    }
}