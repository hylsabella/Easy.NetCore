namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// MQ消费者绑定器
    /// </summary>
    public interface IMqConsumerBinder
    {
        /// <summary>
        /// 发现并绑定消费者
        /// </summary>
        void BindConsumer();
    }
}
