using System;
using System.Collections.Generic;
using System.Text;

namespace Easy.Common.NetCore.MQ
{
    public class MessageInfo<T>
    {
        public string MessageId { get; set; }

        public T Value { get; set; }
    }
}
