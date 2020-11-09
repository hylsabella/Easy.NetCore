namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// 消息回执类型
    /// </summary>
    public enum MqReplyType
    {
        /// <summary>
        /// 确认。发送确认信号，告诉MQ这个消息处理成功，可以从队列移除此消息
        /// </summary>
        Ack = 1,

        /// <summary>
        /// 不确认。发送不确认信号，该消息会从队列移除（若IsRequeue设置为true，那么消息会重新投递到队列头部）
        /// </summary>
        Nack = 2,

        /// <summary>
        /// 拒绝。发送拒绝信号，该消息会从队列清除移除（若IsRequeue设置为true，那么消息会重新投递到队列头部）
        /// </summary>
        Reject = 3,
    }
}
