using System;

namespace Easy.Common.NetCore.MQ
{
    /// <summary>
    /// 延迟队列信息
    /// </summary>
    public class MqExpiresInfo
    {
        public MqExpiresInfo(TimeSpan expires, string expiresRoutingKey, bool isSameExpires)
        {
            this.Expires = expires;
            this.ExpiresRoutingKey = expiresRoutingKey;
            this.IsSameExpires = isSameExpires;
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public TimeSpan Expires { get; private set; }

        /// <summary>
        /// 过期后重新投递的路由值
        /// </summary>
        public string ExpiresRoutingKey { get; private set; }

        /// <summary>
        /// 是否所有消息过期时间一致（默认为：true）
        /// true：基于队列方式实现延迟队列。将队列中所有消息的过期时间（x-message-ttl）设置为一样
        /// false：基于消息方式实现延迟队列。可对队列中消息进行单独设置，每条消息的过期时间TTL可以不同
        /// </summary>
        public bool IsSameExpires { get; private set; } = true;
    }
}