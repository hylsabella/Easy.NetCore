using Easy.Common.NetCore.Extentions;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    public class RabbitMQReceivedBinder : IMqReceivedBinder
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public void Bind()
        {
            var consumers = MqConsumerDispatcher.FindConsumers();

            foreach (var consumer in consumers)
            {
                this.Received(consumer);
            }
        }

        public void Received(MqConsumerExecutor consumerExecutor)
        {
            try
            {
                string exchangeName = consumerExecutor.RouteName;
                string queueName = consumerExecutor.RouteName;

                var channel = RabbitMQManager.Connection.CreateModel();

                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: queueName);

                //进行消费端的限流，限流情况autoAck不能设置为true自动签收，一定要手动签收
                if (!consumerExecutor.AutoAck)
                {
                    channel.BasicQos(prefetchSize: consumerExecutor.PrefetchSize, prefetchCount: consumerExecutor.PrefetchCount, global: false);
                }

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += async (sender, eventArgs) =>
                {
                    MqConsumerExecutedResult result = null;

                    try
                    {
                        if (eventArgs.Body.IsEmpty)
                        {
                            channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                        }

                        var msgJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                        result = await MqConsumerDispatcher.InvokeAsync(consumerExecutor, msgJson);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "调用消费者执行异常");
                    }
                    finally
                    {
                        if (result == null || !result.ReplyType.IsInDefined())
                        {
                            channel.BasicNack(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: false);
                        }
                        else
                        {
                            if (result.ReplyType == MqReplyType.Ack)
                            {
                                //发送消息确认信号，告诉rabbitmq这个消息处理成功，可以从队列清除此消息
                                //同一个会话consumerTag是固定的名字，deliveryTag每次接收消息+1
                                channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                            }
                            else if (result.ReplyType == MqReplyType.Nack)
                            {
                                //requeue会将该消息重新投递到队列头部
                                channel.BasicNack(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: result.IsRequeue);
                            }
                            else if (result.ReplyType == MqReplyType.Reject)
                            {
                                //投递错误时拒绝处理。
                                //BasicNack第二个参数multiple是否应用于多消息，与BasicReject区别就是同时支持多个消息，可以nack该消费者先前接收未ack的所有消息
                                channel.BasicReject(deliveryTag: eventArgs.DeliveryTag, requeue: result.IsRequeue);
                            }
                        }
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: consumerExecutor.AutoAck, consumer: consumer);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "MqReceived异常");
            }
        }
    }
}