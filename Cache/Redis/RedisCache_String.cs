using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace Easy.Common.NetCore.Cache.Redis
{
    public partial class RedisCache : IEasyCache
    {
        #region Get 缓存

        /// <summary>
        /// 获取缓存。如果缓存不存在，返回值为 default(T)
        /// </summary>
        public T Get<T>(string key, int db = 0)
        {
            return Get<T>(key, null, db: db);
        }

        /// <summary>
        /// 获取缓存。（如果缓存不存在，执行数据来源方法，并将值存入缓存中；如果数据来源方法对象为null或者执行结果为null，值不存入缓存）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="createFunc">数据来源方法。如果缓存不存在，执行该方法，并将值存入缓存中；如果该方法对象为null或者执行结果为null，值不存入缓存</param>
        /// <param name="isExpired">是否要过期</param>
        public T Get<T>(string key, Func<T> createFunc, bool isExpired = true, int db = 0)
        {
            if (isExpired)
            {
                return Get(key, createFunc, Expires, db);
            }
            else
            {
                return Get(key, createFunc, TimeSpan.Zero, db);
            }
        }

        /// <summary>
        /// 获取缓存。（如果缓存不存在，执行数据来源方法，并将值存入缓存中；如果数据来源方法对象为null或者执行结果为null，值不存入缓存）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="createFunc">数据来源方法。如果缓存不存在，执行该方法，并将值存入缓存中；如果该方法对象为null或者执行结果为null，值不存入缓存</param>
        /// <param name="expires">过期时间.TimeSpan.Zero：表示不会过期</param>
        public T Get<T>(string key, Func<T> createFunc, TimeSpan expires, int db = 0)
        {
            //保证redis挂了不影响正常逻辑，还是会读数据库
            CheckHelper.NotEmpty(key, "key");

            RedisValue cacheData = RedisValue.Null;
            IDatabase redisdb = null;

            try
            {
                redisdb = RedisManager.Connection.GetDatabase(db);

                cacheData = redisdb.StringGet(key);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Get.RedisCache挂了，key:{key}");

                var memoryCache = new EasyMemoryCache(expires);

                return memoryCache.Get(key, createFunc, expires);
            }

            //缓存中拿到值
            if (!cacheData.IsNullOrEmpty)
            {
                return JsonConvert.DeserializeObject<T>(cacheData);
            }

            //如果createFunc为空，那么直接返回默认值
            if (createFunc == null)
            {
                return default;
            }

            //只有createFunc不为空时才保存数据到缓存中
            T data = createFunc();

            if (data == null)
            {
                return data;
            }

            //保证redis挂了不影响正常逻辑
            try
            {
                string jsonData = JsonConvert.SerializeObject(data);

                if (expires == TimeSpan.Zero)
                {
                    redisdb.StringSet(key, jsonData);
                }
                else
                {
                    redisdb.StringSet(key, jsonData, expires);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Get.StringSet.RedisCache挂了，key:{key}");
            }

            return data;
        }

        #endregion

        #region Set 缓存

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="data">数据</param>
        /// <param name="isExpired">是否要过期</param>
        /// <returns>是否已存入缓存</returns>
        public bool Set<T>(string key, T data, bool isExpired = true, int db = 0)
        {
            if (isExpired)
            {
                return Set(key, data, this.Expires, db);
            }
            else
            {
                return Set(key, data, TimeSpan.Zero, db);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="data">数据</param>
        /// <param name="expires">过期时间.TimeSpan.Zero：表示不会过期</param>
        /// <returns>是否已存入缓存</returns>
        public bool Set<T>(string key, T data, TimeSpan expires, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");

            if (data == null)
            {
                return false;
            }

            TimeSpan? thisExpires = null;

            if (expires != TimeSpan.Zero)
            {
                thisExpires = expires;
            }

            try
            {
                var redisdb = RedisManager.Connection.GetDatabase(db);

                string jsonData = JsonConvert.SerializeObject(data);

                return redisdb.StringSet(key, jsonData, thisExpires);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Set.RedisCache挂了，key:{key}");

                var memoryCache = new EasyMemoryCache(expires);

                return memoryCache.Set(key, data, expires);
            }
        }

        #endregion

        #region 加减值

        /// <summary>
        /// 增加值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">增量</param>
        /// <param name="expires">过期时间。【null】和【TimeSpan.Zero】表示不会过期</param>
        public long Increment(string key, long value = 1, TimeSpan? expires = null, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            long result = redisdb.StringIncrement(key, value);

            if (expires.HasValue && expires != TimeSpan.Zero)
            {
                redisdb.KeyExpire(key, expires.Value);
            }

            return result;
        }

        /// <summary>
        /// 减少值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">减量</param>
        /// <param name="expires">过期时间。【null】和【TimeSpan.Zero】表示不会过期</param>
        /// <returns>减少后的值</returns>
        public long Decrement(string key, long value = 1, TimeSpan? expires = null, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            long result = redisdb.StringDecrement(key, value);

            if (expires.HasValue && expires != TimeSpan.Zero)
            {
                redisdb.KeyExpire(key, expires.Value);
            }

            return result;
        }

        #endregion
    }
}
