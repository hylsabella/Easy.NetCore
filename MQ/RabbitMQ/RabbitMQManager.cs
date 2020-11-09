using Easy.Common.NetCore.Helpers;
using Easy.Common.NetCore.Setting;
using RabbitMQ.Client;
using System;
using System.Configuration;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    /// <summary>
    /// RabbitMQ连接管理
    /// </summary>
    public static class RabbitMQManager
    {
        private readonly static object _lockerConn = new object();
        private static IConnection _connection;

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

                                string hostName = ConfigurationManager.AppSettings["RabbitMQ.HostName"];
                                if (!int.TryParse(ConfigurationManager.AppSettings["RabbitMQ.Port"], out int port))
                                {
                                    throw new Exception("请检查【RabbitMQ.Port】是否为合法端口号");
                                }

                                string userName = ConfigurationManager.AppSettings["RabbitMQ.UserName"];
                                string password = ConfigurationManager.AppSettings["RabbitMQ.Pwd"];

                                bool.TryParse(ConfigurationManager.AppSettings["RabbitMQ.PwdEncrypt"] ?? "", out bool isEncryption);

                                if (isEncryption)
                                {
                                    password = EncryptionHelper.DES解密(password, SecretKeySetting.PlatformDESKey);
                                }

                                var factory = new ConnectionFactory
                                {
                                    HostName = hostName,
                                    Port = port,
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