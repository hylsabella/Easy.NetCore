using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Cache.Redis
{
    public partial class RedisCache : IEasyCache
    {
        /// <summary>
        /// 入队列（非阻塞）
        /// </summary>
        public long QueuePush<T>(string queueName, T data, int db = 0)
        {
            CheckHelper.NotEmpty(queueName, "queueName");
            if (data == null) throw new ArgumentNullException(nameof(data), "不能向redis队列插入空数据");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            string jsonData = JsonConvert.SerializeObject(data);

            return redisdb.ListRightPushAsync(queueName, jsonData).Result;
        }

        /// <summary>
        /// 出队列（非阻塞）【保证数据只会被一个消费者消费】
        /// </summary>
        public T QueuePop<T>(string queueName, int db = 0)
        {
            CheckHelper.NotEmpty(queueName, "queueName");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            var value = redisdb.ListLeftPopAsync(queueName).Result;

            if (value == RedisValue.Null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// 从一个队列RightPop元素到另外一个队列LeftPush
        /// 在一个原子时间内，执行以下两个动作：QueuePop和QueuePush
        /// </summary>
        /// <param name="popQueueName_1nd">RightPop的队列</param>
        /// <param name="pushQueueName_2nd">QueuePush的队列</param>
        public T QueueRPopAndQueueLPush<T>(string popQueueName_1nd, string pushQueueName_2nd, int db = 0)
        {
            CheckHelper.NotEmpty(popQueueName_1nd, "queueName_1nd");
            CheckHelper.NotEmpty(pushQueueName_2nd, "queueName_2nd");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            var value = redisdb.ListRightPopLeftPushAsync(popQueueName_1nd, pushQueueName_2nd).Result;

            if (value == RedisValue.Null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// 返回列表 key 中指定区间内的元素（不从队列中移除成员），区间以偏移量 start 和 stop 指定。
        /// </summary>
        public List<T> LListRange<T>(string queueName, long start = 0, long stop = -1, int db = 0)
        {
            CheckHelper.NotEmpty(queueName, "queueName");

            var result = new List<T>();

            var redisdb = RedisManager.Connection.GetDatabase(db);

            var redisValues = redisdb.ListRangeAsync(queueName, start, stop).Result;

            foreach (var redisValue in redisValues)
            {
                var value = JsonConvert.DeserializeObject<T>(redisValue);

                result.Add(value);
            }

            return result;
        }

        /// <summary>
        /// 对一个列表进行修剪(trim)，就是说，让列表只保留指定区间内的元素，不在指定区间之内的元素都将被删除。
        /// </summary>
        public void LListTrim(string queueName, long start = 0, long stop = -1, int db = 0)
        {
            CheckHelper.NotEmpty(queueName, "queueName");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            redisdb.ListTrimAsync(queueName, start, stop).Wait();
        }

        /// <summary>
        /// 从队列中取出并移除指定范围内的元素（不保证数据只会被一个消费者消费）
        /// </summary>
        public List<T> QueuePopList<T>(string queueName, long stop = -1, int db = 0)
        {
            long start = 0;

            var result = LListRange<T>(queueName, start, stop, db: db);

            LListTrim(queueName, result.Count, -1, db: db);

            return result;
        }

        /// <summary>
        /// 获取队列元素个数
        /// </summary>
        public long GetQueueLength(string queueName, int db = 0)
        {
            CheckHelper.NotEmpty(queueName, "queueName");

            var redisdb = RedisManager.Connection.GetDatabase(db);

            return redisdb.ListLengthAsync(queueName).Result;
        }
    }
}