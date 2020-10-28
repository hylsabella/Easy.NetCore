using CommonServiceLocator;
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
    }
}