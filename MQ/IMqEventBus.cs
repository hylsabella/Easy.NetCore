using System;

namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// 消息总线
    /// </summary>
    public interface IMqEventBus
    {
        /// <summary>
        /// 投递消息到队列
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="routingKey">队列路由值（必须小于255 bytes）</param>
        /// <param name="message">消息</param>
        /// <param name="ackEvent">投递队列成功确认回调</param>
        /// <param name="noAckEvent">投递队列不成功确认回调</param>
        /// <returns>是否成功投递到队列</returns>
        bool Publish<T>(string routingKey, MqMessage<T> message,
           EventHandler<MqEventBusFeedBackEventArgs> ackEvent = null,
           EventHandler<MqEventBusFeedBackEventArgs> noAckEvent = null);
    }

    public class MqEventBusFeedBackEventArgs : EventArgs
    {
        /// <summary>
        /// 已确认消息的序列号，如果multiple为true，则为已确认消息的闭合上限
        /// </summary>
        public ulong DeliveryTag { get; set; }

        /// <summary>
        /// 此确认适用于一条消息还是多条消息
        /// </summary>
        public bool Multiple { get; set; }
    }
}
