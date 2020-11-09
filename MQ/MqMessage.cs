namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// MQ消息体
    /// </summary>
    public class MqMessage<T>
    {
        public string MessageId { get; set; }

        public T Value { get; set; }
    }
}
