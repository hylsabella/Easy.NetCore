using Easy.Common.NetCore.Helpers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Easy.Common.NetCore.MQ.RabbitMQ
{
    /// <summary>
    /// RabbitMQ消费者绑定器
    /// </summary>
    public class RabbitMQConsumerBinder : IMqConsumerBinder
    {
        /// <summary>
        /// 发现并绑定消费者
        /// </summary>
        public virtual void BindConsumer()
        {
            var consumers = MqConsumerDispatcher.DiscoverConsumers();

            foreach (var consumer in consumers)
            {
                this.Received(consumer);
            }
        }

        protected virtual void Received(MqConsumerExecutor consumerExecutor)
        {
            if (consumerExecutor == null || string.IsNullOrWhiteSpace(consumerExecutor?.RouteName))
            {
                throw new ArgumentException("Received(consumerExecutor)不能为空");
            }

            string exchangeName = consumerExecutor.RouteName;
            string queueName = consumerExecutor.RouteName;

            try
            {
                var channel = RabbitMQManager.Connection.CreateModel();

                channel.ExchangeDeclare(exchange: exchangeName, type: RabbitMQExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
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
                    string msgJson = string.Empty;

                    try
                    {
                        if (eventArgs.Body.IsEmpty)
                        {
                            channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                        }

                        msgJson = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                        result = await MqConsumerDispatcher.InvokeAsync(consumerExecutor, msgJson);
                    }
                    catch (Exception ex)
                    {
                        var traceInfo = new { ConsumerExecutor = consumerExecutor, MsgJson = msgJson };

                        LogHelper.Error(ex, $"消费者执行异常：{msgJson}", filename: $"RabbitMQ_{queueName}_Exception");
                    }
                    finally
                    {
                        var traceInfo = new { ConsumerExecutor = consumerExecutor, MsgJson = msgJson };

                        if (result == null || !result.ReplyType.IsInDefined())
                        {
                            LogHelper.Error($"消费者未回馈消息处理情况：{msgJson}", filename: $"RabbitMQ_{queueName}_Unknown");

                            channel.BasicNack(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: false);
                        }
                        else
                        {
                            if (result.ReplyType != MqReplyType.Ack)
                            {
                                LogHelper.Trace($"消费者处理情况：{msgJson}", filename: $"RabbitMQ_{queueName}_{result.ReplyType.ToString()}");
                            }

                            if (result.ReplyType == MqReplyType.Ack)
                            {
                                //发送确认信号，告诉MQ这个消息处理成功，可以从队列移除此消息
                                //同一个会话consumerTag是固定的名字，deliveryTag每次接收消息会+1
                                channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                            }
                            else if (result.ReplyType == MqReplyType.Nack)
                            {
                                //发送不确认信号，该消息会从队列移除（若IsRequeue设置为true，那么消息会重新投递到队列头部）
                                //BasicNack第二个参数multiple是否应用于多消息，与BasicReject区别就是同时支持多个消息，可以nack该消费者先前接收未ack的所有消息
                                channel.BasicNack(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: result.IsRequeue);
                            }
                            else if (result.ReplyType == MqReplyType.Reject)
                            {
                                //发送拒绝信号，该消息会从队列清除移除（若IsRequeue设置为true，那么消息会重新投递到队列头部）
                                channel.BasicReject(deliveryTag: eventArgs.DeliveryTag, requeue: result.IsRequeue);
                            }
                        }
                    }
                };

                channel.BasicConsume(queue: queueName, autoAck: consumerExecutor.AutoAck, consumer: consumer);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex, $"RabbitMQReceivedBinder.Received异常：", filename: $"RabbitMQReceivedBinder.Received_RabbitMQ_{queueName}_Exception");

                throw;
            }
        }
    }
}