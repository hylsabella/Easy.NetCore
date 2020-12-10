using Autofac;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    public static class ContainerBuilderExt
    {
        public static void RegisterRabbitMQEventBus(this ContainerBuilder builder)
        {
            //注册事件总线
            builder.RegisterType<RabbitMQEventBus>().As<IMqEventBus>().PropertiesAutowired().SingleInstance();
        }

        public static void RegisterRabbitMQConsumerBinder(this ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMQConsumerBinder>().As<IMqConsumerBinder>().PropertiesAutowired();
        }
    }
}