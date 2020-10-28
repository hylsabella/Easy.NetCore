using System;
using System.Collections.Generic;
using System.Reflection;

namespace Easy.Common.NetCore.MQ
{
    public class MqConsumerExecutor
    {
        public MqConsumerExecutor(string routeName, ushort prefetchCount, uint prefetchSize, bool autoAck, TypeInfo typeInfo, MethodInfo methodInfo, IList<ParameterDescriptor> parameters)
        {
            this.RouteName = routeName;
            this.PrefetchCount = prefetchCount;
            this.PrefetchSize = prefetchSize;
            this.AutoAck = autoAck;
            this.TypeInfo = typeInfo;
            this.MethodInfo = methodInfo;
            this.Parameters = parameters;
        }

        /// <summary>
        /// 消息路由值
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// 消息预取限制。消费者一次获得的消息数，例如：设置1表示消费者一次只处理一条消息，设置0表示消费者会一次性拿取队列消息
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
        /// 消费者类型信息
        /// </summary>
        public TypeInfo TypeInfo { get; set; }

        /// <summary>
        /// 消费者执行方法信息
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// 消费者执行方法参数
        /// </summary>
        public IList<ParameterDescriptor> Parameters { get; set; }
    }

    public class ParameterDescriptor
    {
        public string Name { get; set; }

        public Type ParameterType { get; set; }
    }

    public class ConsumerExecutorComparer : IEqualityComparer<MqConsumerExecutor>
    {
        public bool Equals(MqConsumerExecutor x, MqConsumerExecutor y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return string.Equals(x.RouteName, y.RouteName) && x.MethodInfo.Equals(y.MethodInfo) && x.TypeInfo.Equals(y.TypeInfo);
        }

        public int GetHashCode(MqConsumerExecutor obj)
        {
            return obj.RouteName.GetHashCode() ^ obj.MethodInfo.GetHashCode() ^ obj.TypeInfo.GetHashCode();
        }
    }
}