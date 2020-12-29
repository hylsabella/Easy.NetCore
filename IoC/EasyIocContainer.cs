using Autofac;
using CommonServiceLocator;
using Easy.Common.NetCore.IoC.Autofac;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.IoC
{
    /// <summary>
    /// EasyIoc通用容器
    /// </summary>
    public static class EasyIocContainer
    {
        private static object _lock = new object();
        public static IServiceLocator Container { get; private set; }

        public static void InitIocContainer(IServiceLocator serviceLocator)
        {
            if (Container == null)
            {
                lock (_lock)
                {
                    if (Container == null)
                    {
                        Container = serviceLocator;
                    }
                }
            }
        }

        public static IEnumerable<TService> GetAllInstances<TService>()
        {
            return Container.GetAllInstances<TService>();
        }

        public static IEnumerable<dynamic> GetAllInstances(Type serviceType)
        {
            return Container.GetAllInstances(serviceType);
        }

        public static TService GetInstance<TService>(string key)
        {
            return Container.GetInstance<TService>(key);
        }

        public static TService GetInstance<TService>()
        {
            return Container.GetInstance<TService>();
        }

        public static object GetInstance(Type serviceType, string key)
        {
            return Container.GetInstance(serviceType, key);
        }

        public static object GetInstance(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }

        public static object GetService(Type serviceType)
        {
            return Container.GetService(serviceType);
        }

        public static bool IsRegistered<TService>()
        {
            return EasyAutofac.Container.IsRegistered<TService>();
        }

        public static bool IsRegistered(Type serviceType)
        {
            return EasyAutofac.Container.IsRegistered(serviceType);
        }

        public static bool IsRegisteredWithKey<TService>(object serviceKey)
        {
            return EasyAutofac.Container.IsRegisteredWithKey<TService>(serviceKey);
        }

        public static bool IsRegisteredWithKey(object serviceKey, Type serviceType)
        {
            return EasyAutofac.Container.IsRegisteredWithKey(serviceKey, serviceType);
        }

        public static bool IsRegisteredWithName(string serviceName, Type serviceType)
        {
            return EasyAutofac.Container.IsRegisteredWithName(serviceName, serviceType);
        }

        public static bool IsRegisteredWithName<TService>(string serviceName)
        {
            return EasyAutofac.Container.IsRegisteredWithName<TService>(serviceName);
        }
    }
}