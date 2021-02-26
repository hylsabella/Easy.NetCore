using Easy.Common.NetCore.Helpers;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.Setting;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;

namespace Easy.Common.NetCore.Cache.Redis
{
    public static class RedisManager
    {
        private readonly static object _lockerConn = new object();
        private readonly static object _lockerServer = new object();
        private static IServer _server;
        private static ConnectionMultiplexer _redis;
        private static readonly IConfiguration _configuration = EasyIocContainer.Container.GetInstance<IConfiguration>();

        public static ConnectionMultiplexer Connection
        {
            get
            {
                if (_redis == null || !_redis.IsConnected)
                {
                    lock (_lockerConn)
                    {
                        if (_redis == null || !_redis.IsConnected)
                        {
                            try
                            {
                                //重新建立连接前，先释放之前的连接对象
                                if (_redis != null)
                                {
                                    _redis.Close(allowCommandsToComplete: true);
                                }

                                string redisHostName = _configuration["appSettings:Redis.HostName"];
                                var configOptions = ConfigurationOptions.Parse(redisHostName);

                                string password = _configuration["appSettings:Redis.Pwd"];

                                bool.TryParse(_configuration["appSettings:Redis.PwdEncrypt"] ?? "", out bool isEncryption);

                                if (isEncryption)
                                {
                                    string pwd = EncryptionHelper.DES解密(password, SecretKeySetting.DES_SecretKey);

                                    configOptions.Password = pwd;
                                }
                                else
                                {
                                    configOptions.Password = password;
                                }

                                configOptions.SyncTimeout = 5000;

                                _redis = ConnectionMultiplexer.Connect(configOptions);

                                if (!_redis.IsConnected) throw new ArgumentException("连接Redis服务器失败！");
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }

                return _redis;
            }
        }

        public static IServer Server
        {
            get
            {
                if (_server == null)
                {
                    lock (_lockerServer)
                    {
                        if (_server == null)
                        {
                            _server = Connection.GetServer(Connection.GetEndPoints()[0]);
                        }
                    }
                }

                return _server;
            }
        }
    }
}