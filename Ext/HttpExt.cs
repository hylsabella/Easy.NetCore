﻿using Easy.Common.NetCore.Exceptions;
using Easy.Common.NetCore.Helpers;
using Easy.Common.NetCore.Serializer;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Easy.Common.NetCore
{
    public static class HttpExt
    {
        public static string HttpPost(this string url, Dictionary<string, object> postParams, Dictionary<string, string> headers = null, HttpPostCfg httpCfg = null)
        {
            IRestResponse response = HttpPostForResponse(url, postParams, headers, ref httpCfg);

            return response.Content;
        }

        public static T HttpPost<T>(this string url, Dictionary<string, object> postParams, Dictionary<string, string> headers = null, HttpPostCfg httpCfg = null)
        {
            string resultStr = url.HttpPost(postParams, headers, httpCfg);

            var result = JsonConvert.DeserializeObject<T>(resultStr);

            return result;
        }

        public static byte[] HttpPostForBytes(this string url, Dictionary<string, object> postParams, Dictionary<string, string> headers = null, HttpPostCfg httpCfg = null)
        {
            IRestResponse restResponse = HttpPostForResponse(url, postParams, headers, ref httpCfg);

            return restResponse.RawBytes;
        }

        private static IRestResponse HttpPostForResponse(string url, Dictionary<string, object> postParams, Dictionary<string, string> headers, ref HttpPostCfg httpCfg)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new FException("url不能为空");
            if (postParams == null || !postParams.Any()) throw new FException("postParams不能为空");

            if (httpCfg == null)
            {
                httpCfg = HttpPostCfg.CreateDefaultHttpPostCfg();
            }
            else if (httpCfg.ReTryCount <= 0)
            {
                httpCfg.ReTryCount = 1;//至少执行一次
            }

            var restRequest = new RestRequest { Method = Method.POST, JsonSerializer = new RestSharpJsonNetSerializer() };
            restRequest.Timeout = 10 * 1000;//连接超时设置为10秒

            restRequest.AddHeader("Accept", "*/*");
            string contentType = GetContentType(httpCfg.ContentType);
            restRequest.AddHeader("Content-Type", contentType);

            if (headers != null && headers.Any())
            {
                restRequest.AddHeaders(headers);
            }

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

            if (httpCfg.Cookies != null && httpCfg.Cookies.Any())
            {
                foreach (var param in httpCfg.Cookies)
                {
                    restRequest.AddCookie(param.Key, param.Value);
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

            return restResponse;
        }

        public static string HttpGet(this string url, Dictionary<string, object> getParams = null, Dictionary<string, string> headers = null, HttpGetCfg httpCfg = null)
        {
            IRestResponse restResponse = HttpGetForResponse(url, getParams, headers, ref httpCfg);

            return restResponse.Content;
        }

        public static T HttpGet<T>(this string url, Dictionary<string, object> getParams = null, Dictionary<string, string> headers = null, HttpGetCfg httpCfg = null)
        {
            string resultStr = url.HttpGet(getParams, headers, httpCfg);

            var result = JsonConvert.DeserializeObject<T>(resultStr);

            return result;
        }

        public static byte[] HttpGetBytes(this string url, Dictionary<string, object> getParams = null, Dictionary<string, string> headers = null, HttpGetCfg httpCfg = null)
        {
            IRestResponse response = HttpGetForResponse(url, getParams, headers, ref httpCfg);

            return response.RawBytes;
        }

        private static IRestResponse HttpGetForResponse(string url, Dictionary<string, object> getParams, Dictionary<string, string> headers, ref HttpGetCfg httpCfg)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new FException("url不能为空");

            if (httpCfg == null)
            {
                httpCfg = HttpGetCfg.CreateDefaultHttpGetCfg();
            }
            else if (httpCfg.ReTryCount <= 0)
            {
                httpCfg.ReTryCount = 1;//至少执行一次
            }

            var restRequest = new RestRequest { Method = Method.GET, JsonSerializer = new RestSharpJsonNetSerializer() };
            restRequest.Timeout = 10 * 1000;//连接超时设置为10秒
            restRequest.AddHeader("Accept", "*/*");

            if (httpCfg.IsFormContentType)
            {
                string contentType = GetContentType(ContentType.Form);
                restRequest.AddHeader("Content-Type", contentType);
            }

            if (headers != null && headers.Any())
            {
                restRequest.AddHeaders(headers);
            }

            if (getParams?.Count > 0)
            {
                foreach (var param in getParams)
                {
                    restRequest.AddParameter(param.Key, param.Value);
                }
            }

            if (httpCfg.Cookies != null && httpCfg.Cookies.Any())
            {
                foreach (var param in httpCfg.Cookies)
                {
                    restRequest.AddCookie(param.Key, param.Value);
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

            return restResponse;
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
        public const int _reTryCount = 2;

        public uint ReTryCount { get; set; } = _reTryCount;

        public ContentType ContentType { get; set; }

        public Dictionary<string, string> Cookies { get; set; }

        public static HttpPostCfg CreateDefaultHttpPostCfg()
        {
            var httpCfg = new HttpPostCfg
            {
                ReTryCount = _reTryCount,
                ContentType = ContentType.Json,
            };

            return httpCfg;
        }
    }

    public class HttpGetCfg
    {
        public const int _reTryCount = 2;

        public uint ReTryCount { get; set; } = _reTryCount;

        public bool IsFormContentType { get; set; }

        public Dictionary<string, string> Cookies { get; set; }

        public static HttpGetCfg CreateDefaultHttpGetCfg()
        {
            var httpCfg = new HttpGetCfg
            {
                ReTryCount = _reTryCount,
                IsFormContentType = false,
            };

            return httpCfg;
        }
    }

    public enum ContentType
    {
        Json = 0,
        Text,
        Form,
    }
}