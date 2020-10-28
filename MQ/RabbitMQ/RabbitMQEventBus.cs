using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    public class RabbitMQEventBus
    {
        /// <summary>
        /// 投递消息到队列
        /// </summary>
        /// <param name="routingKey">队列路由值</param>
        /// <param name="message">Routing key must be shorter than 255 bytes.</param>
        /// <param name="basicAcks">投递队列成功确认回调</param>
        /// <param name="basicNacks">投递队列不成功确认回调</param>
        /// <returns>是否成功投递到队列</returns>
        public static bool Publish<T>(string routingKey, MessageInfo<T> message,
            EventHandler<BasicAckEventArgs> basicAcks = null,
            EventHandler<BasicNackEventArgs> basicNacks = null)
        {
            string exchangeName = routingKey;
            string queueName = routingKey;

            using (var channel = RabbitMQManager.Connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);

                //指定durable:true设置消息队列为持久化队列
                //autoDelete属性针对的是曾经有消费者订阅过但后来取消订阅了，然后该队列会被自动删除，如果一开始就没有订阅者，那么该队列一直存在
                //当Queue中的 autoDelete 属性被设置为true时，那么，当消息接收着宕机，关闭后，消息队列则会删除，消息发送者一直发送消息，当消息接收者重新启动恢复正常后，会接收最新的消息，而宕机期间的消息则会丢失
                //当Quere中的 autoDelete 属性被设置为false时，那么，当消息接收者宕机，关闭后，消息队列不会删除，消息发送者一直发送消息，当消息接收者重新启动恢复正常后，会接收包括宕机期间的消息。
                //exclusive：是否排外的，有两个作用，一：当连接关闭时connection.close()该队列是否会自动删除；二：该队列是否是私有的private，如果不是排外的，可以使用两个消费者都访问同一个队列，没有任何问题，如果是排外的，会对当前队列加锁，其他通道channel是不能访问的，如果强制访问会报异常
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                //绑定消息队列，交换器，routingkey
                channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: queueName);

                //异步confirm方式。这两个事件要写到发生消息之前，否则不会触发
                if (basicAcks != null)
                {
                    channel.BasicAcks += basicAcks;
                }

                if (basicNacks != null)
                {
                    channel.BasicNacks += basicNacks;
                }

                var properties = channel.CreateBasicProperties();
                //将消息标记为持久性
                properties.Persistent = true;

                string messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                //先启用发布服务器确认
                channel.ConfirmSelect();

                channel.BasicPublish(exchange: exchangeName, routingKey: queueName, basicProperties: properties, body: body);

                //等待服务器确认
                bool isConfirms = channel.WaitForConfirms();

                return isConfirms;
            }
        }
    }
}