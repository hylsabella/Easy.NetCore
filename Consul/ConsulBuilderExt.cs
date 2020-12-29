using Consul;
using Easy.Common.NetCore.Startup;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;

namespace Easy.Common.NetCore.Consul
{
    public static class ConsulBuilderExt
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static AppStartup RegisterConsul(this AppStartup startup, ConsulOption consulOption, IApplicationBuilder app = null)
        {
            try
            {
                if (consulOption == null) throw new ArgumentNullException("consulOption不能为空！");
                if (string.IsNullOrWhiteSpace(consulOption.GlobalRegId) ||
                    string.IsNullOrWhiteSpace(consulOption.ServiceName) ||
                    string.IsNullOrWhiteSpace(consulOption.ServiceIP) ||
                    string.IsNullOrWhiteSpace(consulOption.ConsulAddress) ||
                    consulOption.ServicePort <= 0)
                {
                    throw new ArgumentNullException("consulOption各参数不能为空！");
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
                        DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(2),   //服务状态异常后多久注销服务
                        Timeout = TimeSpan.FromSeconds(5),
                        Interval = consulOption.ServiceHealthCheckInterval ?? TimeSpan.FromSeconds(5), //健康检查时间间隔
                        HTTP = !string.IsNullOrWhiteSpace(consulOption.ServiceHealthUrlCheck) ? consulOption.ServiceHealthUrlCheck : null, //健康检查地址
                        TCP = string.IsNullOrWhiteSpace(consulOption.ServiceHealthUrlCheck) ? $"{consulOption.ServiceIP}:{consulOption.ServicePort}" : null//TCP检测健康检查，如果开启了HTTP检测则关闭此项
                    },
                };

                var consulClient = new ConsulClient(x => x.Address = new Uri(consulOption.ConsulAddress));

                //先取消上次注册，重新注册
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();

                //服务注册
                consulClient.Agent.ServiceRegister(registration).Wait();

                //应用程序终止时，服务取消注册
                if (app != null)
                {
                    var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                    if (lifetime == null) throw new ArgumentNullException("lifetime不能为空！");

                    lifetime.ApplicationStopping.Register(() =>
                    {
                        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                    });
                }
                else
                {
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                    };
                }

                return startup;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "注册Consul异常");
                throw;
            }
        }
    }
}