using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Cache.Redis
{
    public partial class RedisCache : IEasyCache
    {
        /// <summary>
        /// 集合-添加成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        /// <param name="db">数据库编号</param>
        public void SetAdd(string key, string memberValue, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            redisdb.SetAddAsync(key, memberValue).Wait();
        }

        /// <summary>
        /// 集合-添加成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValues">成员集合值</param>
        /// <param name="db">数据库编号</param>
        public void SetAdd(string key, string[] memberValues, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                memberValues == null ||
                memberValues.Length <= 0)
            {
                return;
            }

            var redisValues = new List<RedisValue>();

            foreach (string member in memberValues)
            {
                redisValues.Add(member);
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            redisdb.SetAddAsync(key, redisValues.ToArray()).Wait();
        }

        /// <summary>
        /// 集合-获取所有成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="db">数据库编号</param>
        public List<string> GetSetList(string key, int db = 0)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(key))
            {
                return result;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            RedisValue[] redisValues = redisdb.SetMembersAsync(key).Result ?? Array.Empty<RedisValue>();

            foreach (string value in redisValues)
            {
                result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// 集合-获取所有成员个数
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="db">数据库编号</param>
        public long GetSetLength(string key, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SetLengthAsync(key).Result;
        }

        /// <summary>
        /// 集合-删除指定的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员</param>
        /// <param name="db">数据库编号</param>
        public void SetRemove(string key, string memberValue, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            redisdb.SetRemoveAsync(key, memberValue).Wait();
        }

        /// <summary>
        /// 集合-删除指定的多个成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValues">成员集合</param>
        /// <param name="db">数据库编号</param>
        public void SetRemove(string key, string[] memberValues, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                memberValues == null ||
                memberValues.Length <= 0)
            {
                return;
            }

            var redisValues = new List<RedisValue>();

            foreach (string member in memberValues)
            {
                redisValues.Add(member);
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            redisdb.SetRemoveAsync(key, redisValues.ToArray()).Wait();
        }

        /// <summary>
        /// 集合-判断指定的成员是否存在
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员</param>
        /// <param name="db">数据库编号</param>
        public bool IsInSet(string key, string memberValue, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SetContainsAsync(key, memberValue).Result;
        }
    }
}
