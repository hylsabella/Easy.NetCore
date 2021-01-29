using System;
using System.Runtime.Serialization;

namespace Easy.Common.NetCore.Exceptions
{
    /// <summary>
    /// 友好异常（用于显示给前端错误信息）
    /// </summary>
    [Serializable]
    public class FException : Exception
    {
        public FException(string message)
            : base(message)
        {
        }

        public FException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
