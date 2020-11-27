using System;

namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// 标记为消费者
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class MqConsumerAttribute : Attribute
    {
        /// <summary>
        /// 消息路由值
        /// </summary>
        public string RouteName { get; }

        /// <summary>
        /// 消息预取限制。消费者一次获得的消息数，例如：设置1表示消费者一次只处理一条消息，设置0表示消费者会一次性拿取队列所有消息
        /// 预取限制设置太大会导致消费者获得太多任务导致积压，可能导致内存爆满服务崩溃；预取限制设置太小会导致消费者很闲，而队列消息积压
        /// </summary>
        public ushort PrefetchCount { get; }

        /// <summary>
        /// 消息本身的大小。如果设置为0 那么表示对消息本身的大小不限制
        /// </summary>
        public uint PrefetchSize { get; }

        /// <summary>
        /// 是否是自动确认模式，如果为true，那么设置的PrefetchCount和PrefetchSize参数不会生效，消费者会一次性拿取队列所有消息，并且从消息队列移除这些消息
        /// 如果为false，那么会执行PrefetchCount和PrefetchSize参数的配置
        /// </summary>
        public bool AutoAck { get; }

        /// <summary>
        /// 消费者并行数，默认为1（即：创建指定数目的相同消费者，解决消费吞吐量问题）
        /// </summary>
        public uint ParallelNum { get; } = 1;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="routeName">消息路由值</param>
        /// <param name="prefetchCount">消息预取限制。消费者一次获得的消息数，例如：设置1表示消费者一次只处理一条消息，设置0表示消费者会一次性拿取队列消息，
        /// 预取限制设置太大会导致消费者获得太多任务导致积压，可能导致内存爆满服务崩溃；预取限制设置太小会导致消费者很闲，而队列消息积压</param>
        /// <param name="prefetchSize">消息本身的大小。如果设置为0 那么表示对消息本身的大小不限制</param>
        /// <param name="AutoAck">是否是自动确认模式，如果为true，那么设置的PrefetchCount和PrefetchSize参数不会生效，消费者会一次性拿取队列所有消息，并且从消息队列移除这些消息；如果为false，那么会执行PrefetchCount和PrefetchSize参数的配置</param>
        /// <param name="parallelNum">消费者并行数，默认为1（即：创建指定数目的相同消费者，解决消费吞吐量问题）</param>
        public MqConsumerAttribute(string routeName, ushort prefetchCount = 1, uint prefetchSize = 0, bool autoAck = false, uint parallelNum = 1)
        {
            parallelNum = parallelNum <= 0 ? 1 : parallelNum;

            this.RouteName = routeName;
            this.PrefetchCount = prefetchCount;
            this.PrefetchSize = prefetchSize;
            this.AutoAck = autoAck;
            this.ParallelNum = parallelNum;
        }

        public override string ToString()
        {
            return $"{RouteName}-{ParallelNum}";
        }
    }
}