using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Cache.Redis
{
    public partial class RedisCache : IEasyCache
    {
        /// <summary>
        /// 哈希获取值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns>数据键值对</returns>
        public Dictionary<string, string> HashGetAll(string key, int db = 0)
        {
            var result = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(key))
            {
                return result;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            var hashEntrys = redisdb.HashGetAll(key);

            foreach (var entry in hashEntrys)
            {
                result.Add(entry.Name, entry.Value);
            }

            return result;
        }

        /// <summary>
        /// 哈希获取某字段值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns>数据键值对</returns>
        public string HashGet(string key, string hashField, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(hashField))
            {
                return string.Empty;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            RedisValue value = redisdb.HashGet(key, hashField);

            return value;
        }

        /// <summary>
        /// 哈希设置值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="fieldsDic">字段与值的集合</param>
        /// <param name="expires">过期时间.null和TimeSpan.Zero：表示不会过期</param>
        public void HashSet(string key, Dictionary<string, string> fieldsDic, TimeSpan? expires = null, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            var hashFields = new List<HashEntry>();

            foreach (var item in fieldsDic)
            {
                hashFields.Add(new HashEntry(item.Key, item.Value));
            }

            redisdb.HashSet(key, hashFields.ToArray());

            if (expires.HasValue && expires != TimeSpan.Zero)
            {
                redisdb.KeyExpire(key, expires);
            }
        }

        /// <summary>
        /// 哈希设置值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="hashField">字段名</param>
        /// <param name="value">值</param>
        /// <param name="expires">过期时间.null和TimeSpan.Zero：表示不会过期</param>
        public bool HashSet(string key, string hashField, string value, TimeSpan? expires = null, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(hashField))
            {
                return false;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            bool isSuccess = redisdb.HashSet(key, hashField, value);

            if (expires.HasValue && expires != TimeSpan.Zero)
            {
                isSuccess &= redisdb.KeyExpire(key, expires);
            }

            return isSuccess;
        }
    }
}
