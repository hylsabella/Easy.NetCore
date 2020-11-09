using Autofac;
using Easy.Common.NetCore.Extentions;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.MQ
{
    public static class MqConsumerRegister
    {
        public static void RegisterMQConsumer(this ContainerBuilder builder)
        {
            var allTypes = AppDomain.CurrentDomain.GetAllTypes();

            Register<IMqConsumer>(allTypes, builder);
        }

        private static void Register<T>(IEnumerable<Type> allTypes, ContainerBuilder builder) where T : IMqConsumer
        {
            foreach (Type type in allTypes)
            {
                bool isSubClass = typeof(IMqConsumer).IsAssignableFrom(type);

                if (!isSubClass || type == typeof(IMqConsumer))
                {
                    continue;
                }

                builder.RegisterType(type).Named<IMqConsumer>(type.Name).As<IMqConsumer>();
            }
        }
    }
}