using Easy.Common.NetCore.Helpers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Cache.Redis
{
    public partial class RedisCache : IEasyCache
    {
        /// <summary>
        /// 有序集合-添加成员
        /// （如果某个【member】已经是有序集的成员，那么更新这个【member】的【score】值，并通过重新插入这个【member】成员，来保证该【member】在正确的位置上。）
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        /// <param name="score">排序值</param>
        public void SortedSetAdd(string key, string memberValue, double score, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(memberValue))
            {
                return;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            redisdb.SortedSetAdd(key, memberValue, score);
        }

        /// <summary>
        /// 有序集合-返回指定成员的分数，如果成员不存在，那么返回null
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        /// <returns></returns>
        public double? GetSortedSetScore(string key, string memberValue, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(memberValue))
            {
                return null;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetScore(key, memberValue);
        }

        /// <summary>
        /// 有序集合-返回有序集 key 中成员 member 的排名，如果成员不存在，那么返回null
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="member">成员</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>排名，1代表排名第一</returns>
        public long? SortedSetRank(string key, string member, bool isAsc, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");
            CheckHelper.NotEmpty(member, "member");

            Order order = isAsc ? Order.Ascending : Order.Descending;

            var redisdb = RedisManager.Connection.GetDatabase(db);

            long? rank = redisdb.SortedSetRank(key, member, order);

            //排名以 0 为底，也就是说， score 值最小的成员排名为 0，这里+1便于理解
            rank = rank.HasValue ? rank + 1 : null;

            return rank;
        }

        /// <summary>
        /// 有序集合-获取指定区间内的成员
        /// 下标参数 start 和 stop 都以 0 为底，包含 start 和 stop 在内，也就是说，以 0 表示有序集第一个成员，以 1 表示有序集第二个成员，以此类推。
        /// 你也可以使用负数下标，以 -1 表示最后一个成员， -2 表示倒数第二个成员，以此类推。
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="start">开始下标</param>
        /// <param name="stop">结束下标</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>成员集合</returns>
        public string[] GetSortedSetRangeByRank(string key, long start = 0, long stop = -1, bool isAsc = true, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            Order order = isAsc ? Order.Ascending : Order.Descending;

            var values = redisdb.SortedSetRangeByRank(key, start, stop, order);

            if (values == null || values.Length <= 0)
            {
                return new string[] { };
            }

            var result = new List<string>();

            foreach (string value in values)
            {
                result.Add(value);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 有序集合-获取指定区间内的成员和分数
        /// 下标参数 start 和 stop 都以 0 为底，包含 start 和 stop 在内，也就是说，以 0 表示有序集第一个成员，以 1 表示有序集第二个成员，以此类推。
        /// 你也可以使用负数下标，以 -1 表示最后一个成员， -2 表示倒数第二个成员，以此类推。
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="start">开始下标</param>
        /// <param name="stop">结束下标</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="withScores">是否返回Score</param>
        /// <returns>成员集合</returns>
        public Dictionary<string, double> GetSortedSetRangeByRankWithScores(string key, long start = 0, long stop = -1, bool isAsc = true, bool withScores = false, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            Order order = isAsc ? Order.Ascending : Order.Descending;

            SortedSetEntry[] entrys = redisdb.SortedSetRangeByRankWithScores(key, start, stop, order);

            if (entrys == null || entrys.Length <= 0)
            {
                return new Dictionary<string, double>();
            }

            var result = new Dictionary<string, double>();

            foreach (SortedSetEntry entry in entrys)
            {
                result.Add(entry.Element, entry.Score);
            }

            return result;
        }

        /// <summary>
        /// 有序集合-获取【score】值介于 startScore 和 stopScore 之间(包括等于 startScore 或 stopScore )的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="start">开始分数</param>
        /// <param name="stop">结束分数</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>成员集合</returns>
        public string[] GetSortedSetRangeByScore(string key, double startScore = double.NegativeInfinity, double stopScore = double.PositiveInfinity, bool isAsc = true, int db = 0)
        {
            var redisdb = RedisManager.Connection.GetDatabase(db);

            Order order = isAsc ? Order.Ascending : Order.Descending;

            var values = redisdb.SortedSetRangeByScore(key, startScore, stopScore, order: order);

            if (values == null || values.Length <= 0)
            {
                return new string[] { };
            }

            var result = new List<string>();

            foreach (string value in values)
            {
                result.Add(value);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 有序集合-获取集合成员个数
        /// </summary>
        /// <param name="key">集合key</param>
        public long GetSortedSetLength(string key, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetLength(key);
        }

        /// <summary>
        /// 有序集合-移除成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        public bool RemoveSortedSet(string key, string memberValue, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetRemove(key, memberValue);
        }

        /// <summary>
        /// 有序集合-移除成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValues">成员值集合</param>
        public long RemoveSortedSet(string key, string[] memberValues, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                memberValues == null ||
                memberValues.Length <= 0)
            {
                return 0;
            }

            var redisValues = new List<RedisValue>();

            foreach (string member in memberValues)
            {
                redisValues.Add(member);
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetRemove(key, redisValues.ToArray());
        }

        /// <summary>
        /// 有序集合-移除【指定下标】范围内的成员
        /// 下标参数 start 和 stop 都以 0 为底，包含 start 和 stop 在内，也就是说，以 0 表示有序集第一个成员，以 1 表示有序集第二个成员，以此类推。
        /// 你也可以使用负数下标，以 -1 表示最后一个成员， -2 表示倒数第二个成员，以此类推。
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="start">开始下标</param>
        /// <param name="stop">结束下标</param>
        /// <returns>被移除成员的数量</returns>
        public long RemoveRangeSortedSetByRank(string key, long start, long stop, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetRemoveRangeByRank(key, start, stop);
        }

        /// <summary>
        /// 有序集合-移除【score】值介于 min 和 max 之间(包括等于 min 或 max )的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="startScore">开始分数值</param>
        /// <param name="stopScore">结束分数值</param>
        /// <returns>被移除成员的数量</returns>
        public long RemoveRangeSortedSetByScore(string key, double startScore, double stopScore, int db = 0)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return 0;
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetRemoveRangeByScore(key, startScore, stopScore);
        }

        /// <summary>
        /// 为有序集 key 的成员 member 的 score 值加上增量 value
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="member">成员</param>
        /// <param name="value">增量</param>
        /// <returns>增加后的分数</returns>
        public double SortedSetIncrement(string key, string member, double value, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");
            CheckHelper.NotEmpty(member, "member");
            if (value <= 0) throw new ArgumentNullException("value", "value只能为正数！");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetIncrement(key, member, value);
        }

        /// <summary>
        /// 为有序集 key 的成员 member 的 score 值上减掉 value
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="member">成员</param>
        /// <param name="value">减量</param>
        /// <returns>减掉后的分数</returns>
        public double SortedSetDecrement(string key, string member, double value, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");

            CheckHelper.NotEmpty(member, "member");

            if (value <= 0)
            {
                throw new ArgumentNullException("value", "value只能为正数！");
            }

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.SortedSetDecrement(key, member, value);
        }

        /// <summary>
        /// 从有序集合中弹出指定数量的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="count">指定弹出数量</param>
        /// <returns>成员集合</returns>
        public Dictionary<string, double> SortedSetPop(string key, bool isAsc, long count = 1, int db = 0)
        {
            CheckHelper.NotEmpty(key, "key");
            if (count <= 0) throw new ArgumentNullException("count", "count必须大于0！");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            SortedSetEntry[] entrys = redisdb.SortedSetPop(key, count, isAsc ? Order.Ascending : Order.Descending);

            if (entrys == null || entrys.Length <= 0)
            {
                return new Dictionary<string, double>();
            }

            var result = new Dictionary<string, double>();

            foreach (SortedSetEntry entry in entrys)
            {
                result.Add(entry.Element, entry.Score);
            }

            return result;
        }
    }
}