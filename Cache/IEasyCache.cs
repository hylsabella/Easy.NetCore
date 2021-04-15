using Easy.Common.NetCore.Cache.Redis;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Cache
{
    /// <summary>
    /// 缓存接口
    /// </summary>
    public interface IEasyCache
    {
        TimeSpan Expires { get; }

        /// <summary>
        /// 获取缓存。如果缓存不存在，返回值为 default(T)
        /// </summary>
        T Get<T>(string key, int db = 0);

        /// <summary>
        /// 获取缓存。（如果缓存不存在，执行数据来源方法，并将值存入缓存中；如果数据来源方法对象为null或者执行结果为null，值不存入缓存）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="createFunc">数据来源方法。如果缓存不存在，执行该方法，并将值存入缓存中；如果该方法对象为null或者执行结果为null，值不存入缓存</param>
        /// <param name="isExpired">是否要过期</param>
        T Get<T>(string key, Func<T> createFunc, bool isExpired = true, int db = 0);

        /// <summary>
        /// 获取缓存。（如果缓存不存在，执行数据来源方法，并将值存入缓存中；如果数据来源方法对象为null或者执行结果为null，值不存入缓存）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="createFunc">数据来源方法。如果缓存不存在，执行该方法，并将值存入缓存中；如果该方法对象为null或者执行结果为null，值不存入缓存</param>
        /// <param name="expires">过期时间.TimeSpan.Zero：表示不会过期</param>
        T Get<T>(string key, Func<T> createFunc, TimeSpan expires, int db = 0);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="data">数据</param>
        /// <param name="isExpired">是否要过期</param>
        /// <returns>是否已存入缓存</returns>
        bool Set<T>(string key, T data, bool isExpired = true, int db = 0);

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="data">数据</param>
        /// <param name="expires">过期时间.TimeSpan.Zero：表示不会过期</param>
        /// <returns>是否已存入缓存</returns>
        bool Set<T>(string key, T data, TimeSpan expires, int db = 0);

        /// <summary>
        /// 增加值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">增量</param>
        /// <param name="expires">过期时间。【null】和【TimeSpan.Zero】表示不会过期</param>
        /// <returns>增加后的值</returns>
        long Increment(string key, long value = 1, TimeSpan? expires = null, int db = 0);

        /// <summary>
        /// 减少值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">减量</param>
        /// <param name="expires">过期时间。【null】和【TimeSpan.Zero】表示不会过期</param>
        /// <returns>减少后的值</returns>
        long Decrement(string key, long value = 1, TimeSpan? expires = null, int db = 0);


        /// <summary>
        /// 入队列
        /// </summary>
        long QueuePush<T>(string queueName, T data, int db = 0);

        /// <summary>
        /// 出队列（保证数据只会被一个消费者消费）
        /// </summary>
        T QueuePop<T>(string queueName, int db = 0);

        /// <summary>
        /// 从一个队列RightPop元素到另外一个队列LeftPush
        /// 在一个原子时间内，执行以下两个动作：QueuePop和QueuePush
        /// </summary>
        /// <param name="popQueueName_1nd">RightPop的队列</param>
        /// <param name="pushQueueName_2nd">QueuePush的队列</param>
        T QueueRPopAndQueueLPush<T>(string popQueueName_1nd, string pushQueueName_2nd, int db = 0);

        /// <summary>
        /// 返回列表 key 中指定区间内的元素，区间以偏移量 start 和 stop 指定。
        /// </summary>
        List<T> LListRange<T>(string queueName, long start = 0, long stop = -1, int db = 0);

        /// <summary>
        /// 对一个列表进行修剪(trim)，就是说，让列表只保留指定区间内的元素，不在指定区间之内的元素都将被删除。
        /// </summary>
        void LListTrim(string queueName, long start = 0, long stop = -1, int db = 0);

        /// <summary>
        /// 从队列中取出并移除指定范围内的元素（不保证数据只会被一个消费者消费）
        /// </summary>
        List<T> QueuePopList<T>(string queueName, long stop = -1, int db = 0);

        /// <summary>
        /// 获取队列元素个数
        /// </summary>
        long GetQueueLength(string queueName, int db = 0);


        /// <summary>
        /// 哈希获取值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns>数据键值对</returns>
        Dictionary<string, string> HashGetAll(string key, int db = 0);

        /// <summary>
        /// 哈希获取某字段值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns>数据键值对</returns>
        string HashGet(string key, string hashField, int db = 0);

        /// <summary>
        /// 哈希设置值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="fieldsDic">字段与值的集合</param>
        /// <param name="expires">过期时间.null和TimeSpan.Zero：表示不会过期</param>
        void HashSet(string key, Dictionary<string, string> fieldsDic, TimeSpan? expires = null, int db = 0);

        /// <summary>
        /// 哈希设置值
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="hashField">字段名</param>
        /// <param name="value">值</param>
        /// <param name="expires">过期时间.null和TimeSpan.Zero：表示不会过期</param>
        bool HashSet(string key, string hashField, string value, TimeSpan? expires = null, int db = 0);


        /// <summary>
        /// 集合-添加成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        /// <param name="db">数据库编号</param>
        void SetAdd(string key, string memberValue, int db = 0);

        /// <summary>
        /// 集合-添加成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValues">成员集合值</param>
        /// <param name="db">数据库编号</param>
        void SetAdd(string key, string[] memberValues, int db = 0);

        /// <summary>
        /// 集合-获取所有成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="db">数据库编号</param>
        List<string> GetSetList(string key, int db = 0);

        /// <summary>
        /// 集合-获取所有成员个数
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="db">数据库编号</param>
        long GetSetLength(string key, int db = 0);

        /// <summary>
        /// 集合-删除指定的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员</param>
        /// <param name="db">数据库编号</param>
        void SetRemove(string key, string memberValue, int db = 0);

        /// <summary>
        /// 集合-删除指定的多个成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValues">成员集合</param>
        /// <param name="db">数据库编号</param>
        void SetRemove(string key, string[] memberValues, int db = 0);

        /// <summary>
        /// 集合-判断指定的成员是否存在
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员</param>
        /// <param name="db">数据库编号</param>
        bool IsInSet(string key, string memberValue, int db = 0);


        /// <summary>
        /// 有序集合-添加成员
        /// （如果某个【member】已经是有序集的成员，那么更新这个【member】的【score】值，并通过重新插入这个【member】成员，来保证该【member】在正确的位置上。）
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        /// <param name="score">排序值</param>
        void SortedSetAdd(string key, string memberValue, double score, int db = 0);

        /// <summary>
        /// 有序集合-返回指定成员的分数，如果成员不存在，那么返回null
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        /// <returns>分数</returns>
        double? GetSortedSetScore(string key, string memberValue, int db = 0);

        /// <summary>
        /// 有序集合-返回有序集 key 中成员 member 的排名，如果成员不存在，那么返回null
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="member">成员</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>排名，1代表排名第一</returns>
        long? SortedSetRank(string key, string member, bool isAsc, int db = 0);

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
        string[] GetSortedSetRangeByRank(string key, long start = 0, long stop = -1, bool isAsc = true, int db = 0);

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
        Dictionary<string, double> GetSortedSetRangeByRankWithScores(string key, long start = 0, long stop = -1, bool isAsc = true, bool withScores = false, int db = 0);

        /// <summary>
        /// 有序集合-获取【score】值介于 startScore 和 stopScore 之间(包括等于 startScore 或 stopScore )的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="start">开始分数</param>
        /// <param name="stop">结束分数</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>成员集合</returns>
        string[] GetSortedSetRangeByScore(string key, double startScore = double.NegativeInfinity, double stopScore = double.PositiveInfinity, bool isAsc = true, int db = 0);

        /// <summary>
        /// 有序集合-获取集合成员个数
        /// </summary>
        /// <param name="key">集合key</param>
        long GetSortedSetLength(string key, int db = 0);

        /// <summary>
        /// 有序集合-移除成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValue">成员值</param>
        bool RemoveSortedSet(string key, string memberValue, int db = 0);

        /// <summary>
        /// 有序集合-移除成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="memberValues">成员值集合</param>
        long RemoveSortedSet(string key, string[] memberValues, int db = 0);

        /// <summary>
        /// 有序集合-移除【指定下标】范围内的成员
        /// 下标参数 start 和 stop 都以 0 为底，包含 start 和 stop 在内，也就是说，以 0 表示有序集第一个成员，以 1 表示有序集第二个成员，以此类推。
        /// 你也可以使用负数下标，以 -1 表示最后一个成员， -2 表示倒数第二个成员，以此类推。
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="start">开始下标</param>
        /// <param name="stop">结束下标</param>
        /// <returns>被移除成员的数量</returns>
        long RemoveRangeSortedSetByRank(string key, long start, long stop, int db = 0);

        /// <summary>
        /// 有序集合-移除【score】值介于 min 和 max 之间(包括等于 min 或 max )的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="startScore">开始分数值</param>
        /// <param name="stopScore">结束分数值</param>
        /// <returns>被移除成员的数量</returns>
        long RemoveRangeSortedSetByScore(string key, double startScore, double stopScore, int db = 0);

        /// <summary>
        /// 为有序集 key 的成员 member 的 score 值加上增量 value
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="member">成员</param>
        /// <param name="value">增量</param>
        /// <returns>增加后的分数</returns>
        double SortedSetIncrement(string key, string member, double value, int db = 0);

        /// <summary>
        /// 为有序集 key 的成员 member 的 score 值上减掉 value
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="member">成员</param>
        /// <param name="value">减量</param>
        /// <returns>减掉后的分数</returns>
        double SortedSetDecrement(string key, string member, double value, int db = 0);

        /// <summary>
        /// 从有序集合中弹出指定数量的成员
        /// </summary>
        /// <param name="key">集合key</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="count">指定弹出数量</param>
        /// <returns>成员集合</returns>
        Dictionary<string, double> SortedSetPop(string key, bool isAsc, long count = 1, int db = 0);


        /// <summary>
        /// 判断指定Key是否存在
        /// </summary>
        bool KeyExists(string key, int db = 0);

        /// <summary>
        /// 模式匹配相关的Key集合（注意：该操作不要在生产环境使用，有性能问题）
        /// </summary>
        IList<string> Keys(string patternKey, int db = 0);

        /// <summary>
        /// 移除指定Key的数据
        /// </summary>
        void Remove(string key, int db = 0);

        /// <summary>
        /// 设置Key过期时间
        /// </summary>
        bool KeyExpire(string key, TimeSpan expires, int db = 0);

        /// <summary>
        /// 查询Key剩余生存时间
        /// </summary>
        TimeSpan? KeyTimeToLive(string key, int db = 0);

        /// <summary>
        /// 判别在规定时间内调用是否超出次数（如：一秒内只能调用3次）
        /// </summary>
        /// <param name="key">检测Key</param>
        /// <param name="timeType">时间类型</param>
        /// <param name="maxCount">最大可调用次数</param>
        /// <returns>true：已经超出次数 false：未超出次数</returns>
        bool CheckIsOverStep(string key, TimeType timeType, int maxCount);

        /// <summary>
        /// 获取指定资源的锁
        /// </summary>
        /// <param name="resourceKey">锁资源名称</param>
        /// <param name="expires">锁过期时间</param>
        /// <param name="lockInfo">获取到的锁对象</param>
        /// <param name="resourceValue">锁资源对应的值</param>
        /// <returns>是否成功获取锁</returns>
        bool Lock(string resourceKey, TimeSpan expires, out LockInfo lockInfo, string resourceValue = null);

        /// <summary>
        /// 释放指定资源的锁
        /// </summary>
        /// <param name="lockInfo">锁对象</param>
        void Unlock(LockInfo lockInfo);

        /// <summary>
        /// 释放指定资源的锁
        /// </summary>
        /// <param name="resourceKey">资源名称</param>
        /// <param name="resourceValue">锁资源对应的值</param>
        void Unlock(string resourceKey, string resourceValue);
    }
}