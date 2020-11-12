namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// 消费者处理消息结果
    /// </summary>
    public class MqConsumerExecutedResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="messageId">消息全局唯一ID</param>
        /// <param name="replyType">消息回执类型（Ack、Nack、Reject）</param>
        /// <param name="isRequeue">若IsRequeue设置为true，那么消息会重新投递到队列头部</param>
        public MqConsumerExecutedResult(string messageId, MqReplyType replyType, bool isRequeue = false)
        {
            this.MessageId = messageId;
            this.ReplyType = replyType;
            this.IsRequeue = isRequeue;
        }

        /// <summary>
        /// 消息全局唯一ID
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// 消息回执类型（Ack、Nack、Reject）
        /// </summary>
        public MqReplyType ReplyType { get; set; }

        /// <summary>
        /// 若IsRequeue设置为true，那么消息会重新投递到队列头部
        /// </summary>
        public bool IsRequeue { get; set; }
    }
}
