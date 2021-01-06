using Consul;
using Easy.Common.NetCore.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Easy.Common.NetCore.Consul
{
    public static class ConsulBuilderExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static Task _checkServiceIsExistTask;

        public static AppStartup RegisterConsul(this AppStartup startup, ConsulOption consulOption, IApplicationBuilder app = null)
        {
            try
            {
                ConsulClient consulClient = RegisterConsul(consulOption);

                //当应用程序结束时，应注销服务注册
                OnApplicationStopping(consulClient, app, consulOption.GlobalRegId);

                if (_checkServiceIsExistTask == null)
                {
                    _checkServiceIsExistTask = Task.Factory.StartNew(() => CheckServiceIsExistTask(consulClient, consulOption), TaskCreationOptions.LongRunning);
                }

                return startup;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "注册Consul异常");
                throw;
            }
        }

        private static ConsulClient RegisterConsul(ConsulOption consulOption)
        {
            if (consulOption == null) throw new ArgumentNullException("consulOption不能为空！");
            if (string.IsNullOrWhiteSpace(consulOption.ServiceName) ||
                string.IsNullOrWhiteSpace(consulOption.ServiceIP) ||
                string.IsNullOrWhiteSpace(consulOption.ConsulAddress) ||
                consulOption.ServicePort <= 0)
            {
                throw new ArgumentNullException("consulOption各参数不能为空！");
            }

            consulOption.ServiceIP = consulOption.ServiceIP.Trim();
            if (string.IsNullOrWhiteSpace(consulOption.GlobalRegId))
            {
                consulOption.GlobalRegId = $"{consulOption.ServiceIP}:{consulOption.ServicePort}";
            }

            var registration = new AgentServiceRegistration()
            {
                ID = consulOption.GlobalRegId,      // 全局唯一注册Id
                Name = consulOption.ServiceName,    // 服务名
                Address = consulOption.ServiceIP,   // 服务绑定IP
                Port = consulOption.ServicePort,    // 服务绑定端口
                Tags = !string.IsNullOrWhiteSpace(consulOption.ServiceRemark) ? new string[] { consulOption.ServiceRemark } : null,
                Meta = consulOption.Meta,
                Check = new AgentServiceCheck
                {
                    DeregisterCriticalServiceAfter = consulOption.DeregisterCriticalServiceAfter,//服务状态异常后多久注销服务
                    Timeout = TimeSpan.FromSeconds(5),
                    Interval = consulOption.ServiceHealthCheckInterval ?? TimeSpan.FromSeconds(5), //健康检查时间间隔
                    HTTP = !string.IsNullOrWhiteSpace(consulOption.ServiceHealthUrlCheck) ? consulOption.ServiceHealthUrlCheck : null, //健康检查地址
                    TCP = string.IsNullOrWhiteSpace(consulOption.ServiceHealthUrlCheck) ? $"{consulOption.ServiceIP}:{consulOption.ServicePort}" : null//TCP检测健康检查，如果开启了HTTP检测则关闭此项
                },
            };

            var consulClient = new ConsulClient(x => x.Address = new Uri(consulOption.ConsulAddress));

            //服务注册
            consulClient.Agent.ServiceRegister(registration).Wait();

            logger.Trace("Consul 服务注册成功");

            return consulClient;
        }

        private static void CheckServiceIsExistTask(ConsulClient consulClient, ConsulOption consulOption)
        {
            while (true)
            {
                try
                {
                    //默认一分钟检测一次服务是否存在
                    var interval = consulOption.ServiceIsExistCheckInterval ?? TimeSpan.FromMinutes(1);

                    Task.Delay(interval).Wait();

                    var serviceList = consulClient.Catalog.Service(consulOption.ServiceName).Result.Response;

                    bool hasThisService = serviceList.Where(x => x.ServiceID == consulOption.GlobalRegId).Any();

                    if (!hasThisService)
                    {
                        logger.Trace("Consul CheckServiceTask 开始重新注册");
                        consulClient = RegisterConsul(consulOption);
                        logger.Trace("Consul CheckServiceTask 重新注册成功");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "定时检查服务是否存在任务");
                }
            }
        }

        private static void OnApplicationStopping(ConsulClient consulClient, IApplicationBuilder app, string globalRegId)
        {
            if (consulClient == null)
            {
                return;
            }

            //应用程序终止时，服务取消注册
            if (app != null)
            {
                var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                if (lifetime == null) throw new ArgumentNullException("lifetime不能为空！");

                lifetime.ApplicationStopping.Register(() =>
                {
                    logger.Trace("Consul 应用程序终止(ApplicationStopping)，服务已注销");
                    consulClient.Agent.ServiceDeregister(globalRegId).Wait();
                });
            }
            else
            {
                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    logger.Trace("Consul 应用程序终止(ProcessExit)，服务已注销");
                    consulClient.Agent.ServiceDeregister(globalRegId).Wait();
                };
            }
        }
    }
}