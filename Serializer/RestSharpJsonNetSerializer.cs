using Newtonsoft.Json;
using RestSharp.Serializers;

namespace Easy.Common.NetCore.Serializer
{
    public class RestSharpJsonNetSerializer : ISerializer
    {
        public string ContentType { get; set; } = "application/json";

        public string Serialize(object obj)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss",
            };

            return JsonConvert.SerializeObject(obj, serializerSettings);
        }
    }
}