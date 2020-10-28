namespace Easy.Common.NetCore.MQ
{
    public enum MqReplyType
    {
        /// <summary>
        /// 确认。发送消息确认信号，告诉mq这个消息处理成功，可以从队列清除此消息
        /// </summary>
        Ack = 1,

        /// <summary>
        /// 不确认。发送消息不确认信号，该消息会从队列清除（若设置IsRequeue为true，那么消息会重新投递到队列头部）
        /// </summary>
        Nack = 2,

        /// <summary>
        /// 拒绝。发送消息拒绝信号，该消息不会从队列清除（若设置IsRequeue为true，那么消息会重新投递到队列头部）
        /// </summary>
        Reject = 3,
    }
}
