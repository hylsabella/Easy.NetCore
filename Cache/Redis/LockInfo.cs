using System;

namespace Easy.Common.NetCore.Cache.Redis
{
    /// <summary>
    /// redis锁实例
    /// </summary>
    public class LockInfo
    {
        public LockInfo(string resourceKey, string value, TimeSpan validTimeSpan)
        {
            this.ResourceKey = resourceKey;
            this.Value = value;
            this.ValidTimeSpan = validTimeSpan;
        }

        /// <summary>
        /// 锁的Key
        /// </summary>
        public string ResourceKey { get; private set; }

        /// <summary>
        /// 锁的Value
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// 该锁还剩的有效时间
        /// </summary>
        public TimeSpan ValidTimeSpan { get; private set; }
    }
}