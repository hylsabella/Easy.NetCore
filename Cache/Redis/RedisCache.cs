using Easy.Common.NetCore.Extentions;
using Easy.Common.NetCore.Helpers;
using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Easy.Common.NetCore.Cache.Redis
{
    public partial class RedisCache : IEasyCache
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TimeSpan Expires { get; }

        public RedisCache()
        {
            this.Expires = TimeSpan.FromMinutes(60);
        }

        public RedisCache(TimeSpan expires)
        {
            this.Expires = expires;
        }

        #region Key操作

        /// <summary>
        /// 判断指定Key是否存在
        /// </summary>
        public bool KeyExists(string key, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.KeyExists(key);
        }

        /// <summary>
        /// 模式匹配相关的Key集合
        /// </summary>
        public IList<string> Keys(string patternKey, int db = 0)
        {
            CheckHelper.NotEmpty(patternKey, "patternKey");

            var keys = RedisManager.Server.Keys(db, pattern: patternKey);

            return keys.Select(x => x.ToString()).ToList();
        }

        /// <summary>
        /// 移除指定Key的数据
        /// </summary>
        public void Remove(string key, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");

            try
            {
                var redisdb = RedisManager.Connection.GetDatabase(db);

                redisdb.KeyDelete(key);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Remove.RedisCache挂了");

                var memoryCache = new EasyMemoryCache();

                memoryCache.Remove(key);
            }
        }

        /// <summary>
        /// 设置Key过期时间
        /// </summary>
        public bool KeyExpire(string key, TimeSpan expires, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            if (expires == TimeSpan.Zero)
            {
                return false;
            }

            return redisdb.KeyExpire(key, expires);
        }

        /// <summary>
        /// 查询Key剩余生存时间
        /// </summary>
        public TimeSpan? KeyTimeToLive(string key, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.KeyTimeToLive(key);
        }

        #endregion

        #region 流量检测

        /// <summary>
        /// 判别在规定时间内调用是否超出次数（如：一秒内只能调用3次）
        /// </summary>
        /// <param name="key">检测Key</param>
        /// <param name="timeType">时间类型</param>
        /// <param name="maxCount">最大可调用次数</param>
        /// <returns>true：已经超出次数 false：未超出次数</returns>
        public bool CheckIsOverStep(string key, TimeType timeType, int maxCount)
        {
            CheckHelper.NotEmpty(key, "key");

            if (!timeType.IsInDefined()) throw new ArgumentException("时间类型不合法！");
            if (maxCount < 0) throw new ArgumentException("最大次数必须大于等于0！");

            //缓存Key
            string cacheKey = "IsOverStep:";
            //缓存过期时间
            TimeSpan? expires = null;

            switch (timeType)
            {
                case TimeType.秒:
                    string second = DateTime.Now.ToString("yyyyMMddHHmmss");
                    cacheKey += $"{key}:{second}";
                    expires = TimeSpan.FromSeconds(10);
                    break;
                case TimeType.分:
                    string minute = DateTime.Now.ToString("yyyyMMddHHmm");
                    cacheKey += $"{key}:{minute}";
                    expires = TimeSpan.FromMinutes(5);
                    break;
                case TimeType.时:
                    string hour = DateTime.Now.ToString("yyyyMMddHH");
                    cacheKey += $"{key}:{hour}";
                    expires = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(5));
                    break;
                case TimeType.天:
                    string day = DateTime.Now.ToString("yyyyMMdd");
                    cacheKey += $"{key}:{day}";
                    expires = TimeSpan.FromDays(1).Add(TimeSpan.FromMinutes(5));
                    break;
                case TimeType.月:
                    string month = DateTime.Now.ToString("yyyyMM");
                    cacheKey += $"{key}:{month}";
                    expires = TimeSpan.FromDays(30).Add(TimeSpan.FromMinutes(5));
                    break;
                default:
                    break;
            }

            return this.CheckIsOverStep(cacheKey, maxCount, expires.Value);
        }

        private bool CheckIsOverStep(string cacheKey, int maxCount, TimeSpan expires)
        {
            CheckHelper.NotEmpty(cacheKey, "cacheKey");

            //查询该key是否存在缓存，不存在则设置缓存为0，期限为10秒
            int currValue = Get(cacheKey, () => { return 0; }, expires);

            //超过最大访问次数
            if (currValue >= maxCount)
            {
                return true;
            }

            var redisdb = RedisManager.Connection.GetDatabase();

            var trans = redisdb.CreateTransaction();

            trans.AddCondition(Condition.KeyExists(cacheKey));

            trans.StringIncrementAsync(cacheKey);

            trans.KeyExpireAsync(cacheKey, expires);

            trans.Execute();

            return false;
        }

        #endregion

        #region 锁管理

        /// <summary>
        /// 获取指定资源的锁
        /// </summary>
        /// <param name="resourceKey">锁资源名称</param>
        /// <param name="expires">锁过期时间</param>
        /// <param name="lockInfo">获取到的锁对象</param>
        /// <param name="resourceValue">锁资源对应的值</param>
        /// <returns>是否成功获取锁</returns>
        public bool Lock(string resourceKey, TimeSpan expires, out LockInfo lockInfo, string resourceValue = null)
        {
            RedisLock redisLock = new RedisLock(RedisManager.Connection);

            return redisLock.Lock(resourceKey, expires, out lockInfo, resourceValue);
        }

        /// <summary>
        /// 释放指定资源的锁
        /// </summary>
        /// <param name="lockInfo">锁对象</param>
        public void Unlock(LockInfo lockInfo)
        {
            RedisLock redisLock = new RedisLock(RedisManager.Connection);

            redisLock.Unlock(lockInfo);
        }

        /// <summary>
        /// 释放指定资源的锁
        /// </summary>
        /// <param name="resourceKey">资源名称</param>
        /// <param name="resourceValue">锁资源对应的值</param>
        public void Unlock(string resourceKey, string resourceValue)
        {
            RedisLock redisLock = new RedisLock(RedisManager.Connection);

            redisLock.Unlock(resourceKey, resourceValue);
        }

        #endregion
    }
}