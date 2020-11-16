using Autofac;
using CommonServiceLocator;
using Easy.Common.NetCore.Cache;
using Easy.Common.NetCore.Cache.Redis;
using Easy.Common.NetCore.Enums;
using Easy.Common.NetCore.IoC;
using Easy.Common.NetCore.IoC.Autofac;
using Easy.Common.NetCore.MQ;
using Easy.Common.NetCore.Security.DefendAttack;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Easy.Common.NetCore.Startup
{
    /// <summary>
    /// 启动扩展
    /// </summary>
    public static class AppStartupExt
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化MEF容器
        /// </summary>
        /// <param name="subDirName">dll目录名称</param>
        public static AppStartup InitMEF(this AppStartup startup, string subDirName = "")
        {
            var catalog = new AggregateCatalog();

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, subDirName ?? string.Empty);

            if (!Directory.Exists(path)) throw new ArgumentException("初始化MEF目录未找到");

            catalog.Catalogs.Add(new DirectoryCatalog(path));

            var container = new CompositionContainer(catalog, true);

            EasyMefContainer.InitMefContainer(container);

            return startup;
        }

        /// <summary>
        /// 初始化MEF容器
        /// </summary>
        public static AppStartup InitMEF(this AppStartup startup, params Assembly[] assemblyList)
        {
            if (assemblyList == null || assemblyList.Length <= 0)
            {
                return startup;
            }

            var assemblyDistinctList = assemblyList.Distinct();

            var catalog = new AggregateCatalog();

            foreach (var assembly in assemblyDistinctList)
            {
                catalog.Catalogs.Add(new AssemblyCatalog(assembly));
            }

            var container = new CompositionContainer(catalog, true);

            EasyMefContainer.InitMefContainer(container);

            return startup;
        }

        /// <summary>
        /// 初始化MEF容器
        /// </summary>
        public static AppStartup InitMEF(this AppStartup startup, params Type[] typeList)
        {
            if (typeList == null || typeList.Length <= 0)
            {
                return startup;
            }

            var typeDistinctList = typeList.Distinct();

            var catalog = new AggregateCatalog();

            foreach (var type in typeDistinctList)
            {
                catalog.Catalogs.Add(new TypeCatalog(type));
            }

            var container = new CompositionContainer(catalog, true);

            EasyMefContainer.InitMefContainer(container);

            return startup;
        }

        /// <summary>
        /// 初始化全局IoC容器
        /// </summary>
        public static AppStartup InitIoC(this AppStartup startup, IServiceLocator serviceLocator)
        {
            if (serviceLocator == null) throw new Exception("IServiceLocator对象不能为空");

            EasyIocContainer.InitIocContainer(serviceLocator);

            return startup;
        }

        public static AppStartup RegExtraIoC(this AppStartup startup, ContainerBuilder builder = null)
        {
            if (builder == null)
            {
                new EasyAutofac().RegExtraIoc(hasExtraIocReg: true);
            }
            else
            {
                new EasyAutofac(builder).RegExtraIoc(hasExtraIocReg: true);
            }

            return startup;
        }

        /// <summary>
        /// 初始化缓存服务
        /// </summary>
        public static AppStartup RegRedisCache(this AppStartup startup, ContainerBuilder builder = null, TimeSpan? cacheExpires = null)
        {
            if (builder == null && EasyAutofac.Container != null) throw new Exception("注册Redis必须在初始化IOC容器生成之前完成！");

            RedisCache redisCache = null;

            if (cacheExpires == null)
            {
                redisCache = new RedisCache();
            }
            else
            {
                redisCache = new RedisCache(cacheExpires.Value);
            }

            if (builder == null)
            {
                EasyAutofac.ContainerBuilder.Register(c => redisCache).As<IEasyCache>().SingleInstance();
            }
            else
            {
                builder.Register(c => redisCache).As<IEasyCache>().SingleInstance();
            }

            return startup;
        }

        /// <summary>
        /// 测试缓存连接状态
        /// </summary>
        public static AppStartup CheckRedis(this AppStartup startup)
        {
            try
            {
                //测试redis是否连接成功
                var dataBase = RedisManager.Connection.GetDatabase(0);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "连接Redis服务器失败");
            }

            return startup;
        }



        public static AppStartup RegConfig(this AppStartup startup, IConfiguration configuration, ContainerBuilder builder = null)
        {
            if (configuration == null) throw new Exception("配置configuration不能为空");

            if (builder != null)
            {
                builder.Register(c => configuration).As<IConfiguration>().SingleInstance();
            }
            else
            {
                EasyAutofac.ContainerBuilder.Register(c => configuration).As<IConfiguration>().SingleInstance();
            }

            return startup;
        }

        public static AppStartup UseNLog(this AppStartup startup, string configFilePath)
        {
            if (!File.Exists(configFilePath)) throw new FileNotFoundException("未找到nlog配置文件");

            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configFilePath);

            return startup;
        }

        /// <summary>
        /// 初始化MQ消费者事件绑定（在IOC容器生成后执行）
        /// </summary>
        public static AppStartup BindMqConsumer(this AppStartup startup)
        {
            if (EasyAutofac.Container == null) throw new Exception("初始化MQ消费者事件绑定必须在IOC容器生成后执行！");

            var binder = EasyIocContainer.GetInstance<IMqConsumerBinder>();

            binder.BindConsumer();

            return startup;
        }

        /// <summary>
        /// 获取需要防御流量攻击的【Action】
        /// </summary>
        public static AppStartup InitLimitAttack(this AppStartup startup, WebType webType, Assembly assembly)
        {
            var limitAttackModelList = new List<DefendAttackModel>();

            if (webType == WebType.Mvc)
            {
                limitAttackModelList = DefendAttack_Mvc.GetLimitAttackModel(assembly);
            }
            else if (webType == WebType.WebApi)
            {
                limitAttackModelList = DefendAttack_WebApi.GetLimitAttackModel(assembly);
            }

            DefendAttackContainer.InitDefendAttackList(limitAttackModelList, assembly.GetName().Name);

            return startup;
        }

        /// <summary>
        /// 初始化机器线程池配置
        /// </summary>
        /// <param name="minWorkerThreads">最小工作线程数（每个逻辑CPU核心最优应设置为50，例如当前是4核CPU，那么该参数应为：4 * 50 = 200）</param>
        /// <param name="minIoThreads">最小IO线程数（每个逻辑CPU核心最优应设置为50，例如当前是4核CPU，那么该参数应为：4 * 50 = 200）</param>
        public static AppStartup InitMachineConfig(this AppStartup startup, int minWorkerThreads = 200, int minIoThreads = 200)
        {
            ThreadPool.SetMinThreads(minWorkerThreads, minIoThreads);

            ThreadPool.GetMinThreads(out int minWorkThread, out int minIOThread);
            ThreadPool.GetMaxThreads(out int maxWorkThread, out int maxIOThread);
            ThreadPool.GetAvailableThreads(out int workThread, out int completeThread);

            string result = Environment.NewLine;
            result += "最大工作线程：" + maxWorkThread + "，最大IO线程：" + maxIOThread + Environment.NewLine;
            result += "最小工作线程：" + minWorkThread + "，最小IO线程：" + minIOThread + Environment.NewLine;
            result += "可用工作线程：" + workThread + "，可用IO线程：" + completeThread + Environment.NewLine;
            result += Environment.NewLine;

            logger.Info(result);

            return startup;
        }
    }
}