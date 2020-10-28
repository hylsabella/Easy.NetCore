using StackExchange.Redis;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Easy.Common.NetCore.Cache.Redis
{
    /// <summary>
    /// Redis锁
    /// </summary>
    public class RedisLock
    {
        /// <summary>
        /// 默认重试次数
        /// </summary>
        private const int retryCount = 3;

        /// <summary>
        /// 默认重试延迟
        /// </summary>
        private readonly TimeSpan retryDelay = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// redis服务器集合
        /// </summary>
        private readonly ConnectionMultiplexer[] redisServerList;

        /// <summary>
        /// 获得锁的临界值：投票半数则通过
        /// </summary>
        protected int 获得锁的临界值 { get { return (redisServerList.Length / 2) + 1; } }

        public RedisLock(params ConnectionMultiplexer[] redisServer)
        {
            redisServerList = redisServer;
        }

        public bool Lock(string resourceKey, TimeSpan expires, out LockInfo lockInfo, string resourceValue = null)
        {
            string value = resourceValue;

            if (string.IsNullOrWhiteSpace(value))
            {
                value = this.CreateUniqueLockId();
            }

            LockInfo innerLock = null;

            bool isSuccess = this.RetryGetLock(retryCount, retryDelay, () =>
            {
                try
                {
                    int 当前已获得的锁 = 0;
                    var startTime = DateTime.Now;

                    this.Foreach_RedisServer(redisServer =>
                    {
                        if (this.LockInstance(redisServer, resourceKey, value, expires))
                        {
                            当前已获得的锁 += 1;
                        }
                    });

                    var drift = Convert.ToInt32((expires.TotalMilliseconds * 0.01) + 2);

                    //如果validTimeSpan大于0，表示在超时之前已经获得锁，该锁是有效的
                    var validTimeSpan = expires - (DateTime.Now - startTime) - TimeSpan.FromMilliseconds(drift);

                    if (当前已获得的锁 >= 获得锁的临界值 && validTimeSpan.TotalMilliseconds > 0)
                    {
                        innerLock = new LockInfo(resourceKey, value, validTimeSpan);

                        return true;
                    }
                    else
                    {
                        //如果没有获取到一半以上的服务器锁，那么要尽快释放之前获得的锁
                        this.Foreach_RedisServer(redisServer =>
                        {
                            this.UnlockInstance(redisServer, resourceKey, value);
                        });

                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            });

            lockInfo = innerLock;

            return isSuccess;
        }

        public void Unlock(LockInfo lockInfo)
        {
            if (lockInfo == null ||
                string.IsNullOrWhiteSpace(lockInfo.ResourceKey) ||
                string.IsNullOrWhiteSpace(lockInfo.Value))
            {
                return;
            }

            this.Foreach_RedisServer(redisServer =>
            {
                this.UnlockInstance(redisServer, lockInfo.ResourceKey, lockInfo.Value);
            });
        }

        public void Unlock(string resourceKey, string resourceValue)
        {
            if (string.IsNullOrWhiteSpace(resourceKey) ||
                string.IsNullOrWhiteSpace(resourceValue))
            {
                return;
            }

            this.Foreach_RedisServer(redisServer =>
            {
                this.UnlockInstance(redisServer, resourceKey, resourceValue);
            });
        }

        protected bool LockInstance(ConnectionMultiplexer redisServer, string resourceKey, string value, TimeSpan expires)
        {
            bool isSuccess;

            try
            {
                //对应redis的SETNX命令
                isSuccess = redisServer.GetDatabase().StringSet(resourceKey, value, expires, When.NotExists);
            }
            catch (Exception)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        /// <summary>
        /// 解锁脚本（解锁前需要判断是否是自己锁定的）
        /// </summary>
        private const string unlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";

        protected void UnlockInstance(ConnectionMultiplexer redisServer, string resourceKey, string value)
        {
            RedisKey[] key = { resourceKey };

            RedisValue[] values = { value };

            redisServer.GetDatabase().ScriptEvaluate(unlockScript, key, values);
        }

        protected void Foreach_RedisServer(Action<ConnectionMultiplexer> regAction)
        {
            foreach (var redisServer in redisServerList)
            {
                regAction(redisServer);
            }
        }

        protected bool RetryGetLock(int retryCount, TimeSpan retryDelay, Func<bool> action)
        {
            int maxRetryDelay = (int)retryDelay.TotalMilliseconds;

            Random rnd = new Random();

            int currRetry = 0;

            while (currRetry++ < retryCount)
            {
                if (action())
                {
                    return true;
                }

                Thread.Sleep(rnd.Next(maxRetryDelay));
            }

            return false;
        }

        protected string CreateUniqueLockId()
        {
            return Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.GetType().FullName);

            sb.AppendLine("Registered Connections:");
            foreach (var redisServer in redisServerList)
            {
                sb.AppendLine(redisServer.GetEndPoints().First().ToString());
            }

            return sb.ToString();
        }
    }
}