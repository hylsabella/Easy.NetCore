namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    /// <summary>
    /// RabbitMQ消息投递类型
    /// </summary>
    public static class RabbitMQExchangeType
    {
        /// <summary>
        /// 明确的路由规则：消费端绑定的队列名称必须和消息发布时指定的路由名称一致
        /// </summary>
        public const string Direct = "direct";

        /// <summary>
        /// 模式匹配的路由规则：支持通配符
        /// </summary>
        public const string Topic = "topic";

        /// <summary>
        /// 消息广播，将消息分发到exchange上绑定的所有队列上
        /// </summary>
        public const string Fanout = "fanout";
    }
}
