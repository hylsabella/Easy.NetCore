using Autofac;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    public static class ContainerBuilderExt
    {
        /// <summary>
        /// 注册RabbitMQ事件总线
        /// </summary>
        public static void RegisterRabbitMQEventBus(this ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMQEventBus>().As<IMqEventBus>().PropertiesAutowired().SingleInstance();
        }

        /// <summary>
        /// 注册RabbitMQ消费者绑定器
        /// </summary>
        public static void RegisterRabbitMQConsumerBinder(this ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMQConsumerBinder>().As<IMqConsumerBinder>().PropertiesAutowired();
        }
    }
}