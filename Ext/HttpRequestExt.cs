using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Easy.Common.NetCore
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

        public static JObject GetRequestJsonParam(this HttpRequest Request)
        {
            var resultJObject = new JObject();

            if (Request == null)
            {
                return new JObject();
            }

            if (string.Equals(Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string key in Request.Query.Keys)
                {
                    resultJObject.Add(key, Request.Query[key].ToString());
                }
            }
            else if (string.Equals(Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            {
                Request.TryGetHeader("Content-Type", out string contentType);
                string streamString = Request.Body.GetStreamString();

                if (ContentTypeContains(contentType, "application/json"))
                {
                    resultJObject = StreamStringToJsonDict(streamString, contentType);
                }
                else if (ContentTypeContains(contentType, "text/plain"))
                {
                    try
                    {
                        resultJObject = StreamStringToJsonDict(streamString, contentType);
                    }
                    catch (Exception)
                    {
                        resultJObject.RemoveAll();
                        //不是JSON格式
                        streamString = streamString.TrimStart('?');
                        var queryString = HttpUtility.ParseQueryString(streamString);

                        foreach (string key in queryString.AllKeys)
                        {
                            resultJObject.Add(key, queryString[key]);
                        }
                    }
                }
                else if (ContentTypeContains(contentType, "application/x-www-form-urlencoded"))
                {
                    var queryString = HttpUtility.ParseQueryString(streamString);

                    foreach (string key in queryString.AllKeys)
                    {
                        resultJObject.Add(key, queryString[key]);
                    }
                }
                else if (ContentTypeContains(contentType, "application/xml"))
                {
                    throw new Exception("该方法不支持解析XML格式");
                }
            }
            else
            {
                throw new Exception("HttpMethod只能是Get和Post");
            }

            return resultJObject;
        }

        public static Dictionary<string, string> GetRequestGet(this HttpRequest Request)
        {
            var resultDict = new Dictionary<string, string>();

            foreach (string key in Request.Query.Keys)
            {
                resultDict.Add(key, Request.Query[key].ToString());
            }

            return resultDict;
        }

        public static Dictionary<string, string> GetRequestForm(this HttpRequest Request)
        {
            var resultDict = new Dictionary<string, string>();

            foreach (string key in Request.Form.Keys)
            {
                resultDict.Add(key, Request.Form[key].ToString());
            }

            return resultDict;
        }

        /// <summary>
        /// 获取Xml的Json数据
        /// </summary>
        public static string GetPayCallbackXmlInfo(this Stream stream)
        {
            string streamString = GetStreamString(stream);

            if (string.IsNullOrWhiteSpace(streamString))
            {
                return string.Empty;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(streamString);

            string jsonText = JsonConvert.SerializeXmlNode(doc);

            return jsonText;
        }

        private static bool ContentTypeContains(string contentType, string hasValue)
        {
            if (string.IsNullOrWhiteSpace(contentType)) throw new Exception("contentType不能为空");
            if (hasValue == null) throw new Exception("hasValue不能为空");

            return contentType.IndexOf(hasValue, StringComparison.OrdinalIgnoreCase) > -1;
        }

        private static JObject StreamStringToJsonDict(string streamString, string contentType)
        {
            var jObject = (JObject)JsonConvert.DeserializeObject(streamString);
            if (jObject == null) throw new Exception($"contentType{contentType}，从InputStream中获取jObject为空");

            return jObject;
        }

        /// <summary>
        /// 读流
        /// </summary>
        public static string GetStreamString(this Stream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            StreamReader reader = new StreamReader(stream);
            string result = reader.ReadToEndAsync().Result.Trim();

            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return result;
        }
    }
}
