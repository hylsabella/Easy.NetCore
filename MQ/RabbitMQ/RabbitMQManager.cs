using Easy.Common.NetCore.Helpers;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.Setting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    public static class RabbitMQManager
    {
        private readonly static object _lockerConn = new object();
        private static IConnection _connection;
        private static readonly IConfiguration _configuration = EasyIocContainer.Container.GetInstance<IConfiguration>();

        public static IConnection Connection
        {
            get
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    lock (_lockerConn)
                    {
                        if (_connection == null || !_connection.IsOpen)
                        {
                            try
                            {
                                //重新建立连接前，先释放之前的连接对象
                                if (_connection != null)
                                {
                                    _connection.Close();
                                }

                                string hostName = _configuration["appSettings:RabbitMQ.HostName"];
                                string userName = _configuration["appSettings:RabbitMQ.UserName"];
                                string password = _configuration["appSettings:RabbitMQ.Pwd"];

                                bool.TryParse(_configuration["appSettings:RabbitMQ.PwdEncrypt"] ?? "", out bool isEncryption);

                                if (isEncryption)
                                {
                                    password = EncryptionHelper.DES解密(password, SecretKeySetting.PlatformDESKey);
                                }

                                var factory = new ConnectionFactory
                                {
                                    HostName = hostName,
                                    UserName = userName,
                                    Password = password,
                                };

                                _connection = factory.CreateConnection();

                                if (!_connection.IsOpen) throw new ArgumentException("连接RabbitMQ服务器失败！");
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("请检查【RabbitMQ】数据库配置", ex);
                            }
                        }
                    }
                }

                return _connection;
            }
        }
    }
}