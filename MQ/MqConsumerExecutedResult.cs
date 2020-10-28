namespace Easy.Common.NetCore.MQ
{
    public class MqConsumerExecutedResult
    {
        public MqConsumerExecutedResult(string messageId, MqReplyType replyType, bool isRequeue)
        {
            this.MessageId = messageId;
            this.ReplyType = replyType;
            this.IsRequeue = isRequeue;
        }

        public string MessageId { get; set; }

        public MqReplyType ReplyType { get; set; }

        public bool IsRequeue { get; set; }
    }
}
