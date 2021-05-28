using Newtonsoft.Json;

namespace Easy.Common.NetCore
{
    public static class ObjectExt
    {
        public static string ToJsonStr(this object value)
        {
            if (value == null) return null;

            return JsonConvert.SerializeObject(value);
        }
    }
}