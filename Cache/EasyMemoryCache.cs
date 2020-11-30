using Easy.Common.NetCore.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NLog;
using System;

namespace Easy.Common.NetCore.Cache
{
    public class EasyMemoryCache
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public TimeSpan Expires { get; }

        public EasyMemoryCache()
        {
            this.Expires = TimeSpan.FromMinutes(60);
        }

        public EasyMemoryCache(TimeSpan expires)
        {
            this.Expires = expires;
        }

        /// <summary>
        /// 获取缓存。如果缓存不存在，返回值为 default(T)
        /// </summary>
        public T Get<T>(string key)
        {
            return Get<T>(key, null);
        }

        /// <summary>
        /// 获取缓存。（如果缓存不存在，执行数据来源方法，并将值存入缓存中；如果数据来源方法对象为null或者执行结果为null，值不存入缓存）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="createFunc">数据来源方法。如果缓存不存在，执行该方法，并将值存入缓存中；如果该方法对象为null或者执行结果为null，值不存入缓存</param>
        /// <param name="isExpired">是否要过期</param>
        public T Get<T>(string key, Func<T> createFunc, bool isExpired = true)
        {
            if (isExpired)
            {
                return Get(key, createFunc, Expires);
            }
            else
            {
                return Get(key, createFunc, TimeSpan.Zero);
            }
        }

        /// <summary>
        /// 获取缓存。（如果缓存不存在，执行数据来源方法，并将值存入缓存中；如果数据来源方法对象为null或者执行结果为null，值不存入缓存）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="createFunc">数据来源方法。如果缓存不存在，执行该方法，并将值存入缓存中；如果该方法对象为null或者执行结果为null，值不存入缓存</param>
        /// <param name="expires">过期时间.TimeSpan.Zero：表示不会过期</param>
        public T Get<T>(string key, Func<T> createFunc, TimeSpan expires)
        {
            //保证MemoryCache挂了不影响正常逻辑，还是会读数据库
            CheckHelper.NotEmpty(key, "key");

            string cacheData;

            try
            {
                cacheData = _memoryCache.Get<string>(key);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Get.MemoryCache挂了，key:{key}");

                T defaultData = default;

                if (createFunc != null)
                {
                    defaultData = createFunc();
                }

                return defaultData;
            }

            //缓存中拿到值
            if (!string.IsNullOrWhiteSpace(cacheData))
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

            //保证MemoryCache挂了不影响正常逻辑
            try
            {
                string jsonData = JsonConvert.SerializeObject(data);

                if (expires == TimeSpan.Zero)
                {
                    _memoryCache.Set(key, jsonData);
                }
                else
                {
                    _memoryCache.Set(key, jsonData, expires);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Get.StringSet.MemoryCache挂了，key:{key}");
            }

            return data;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="data">数据</param>
        /// <param name="isExpired">是否要过期</param>
        /// <returns>是否已存入缓存</returns>
        public bool Set<T>(string key, T data, bool isExpired = true)
        {
            if (isExpired)
            {
                return Set(key, data, this.Expires);
            }
            else
            {
                return Set(key, data, TimeSpan.Zero);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="data">数据</param>
        /// <param name="expires">过期时间.TimeSpan.Zero：表示不会过期</param>
        /// <returns>是否已存入缓存</returns>
        public bool Set<T>(string key, T data, TimeSpan expires)
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
                string jsonData = JsonConvert.SerializeObject(data);

                if (thisExpires.HasValue)
                {
                    var result = _memoryCache.Set(key, jsonData, thisExpires.Value);
                    return !string.IsNullOrWhiteSpace(result);
                }
                else
                {
                    var result = _memoryCache.Set(key, jsonData);
                    return !string.IsNullOrWhiteSpace(result);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Set.MemoryCache挂了，key:{key}");

                return false;
            }
        }

        /// <summary>
        /// 移除指定Key的数据
        /// </summary>
        public void Remove(string key)
        {
            CheckHelper.NotEmpty(key, "key");

            try
            {
                _memoryCache.Remove(key);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Remove.MemoryCache挂了");
            }
        }
    }
}