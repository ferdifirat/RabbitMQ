using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.EventBus;
using RabbitMQ.EventBus.Core;
using System.Net;
using System.Text;

namespace RabbitMQ.Server.Messaging.Consumers
{
    public class EventBusJobApiConsumer
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;

        public EventBusJobApiConsumer(IRabbitMQPersistentConnection persistentConnection)
        {
            _persistentConnection = persistentConnection;
        }

        public void Consume(string queue, bool singleActiveConsumer = false)
        {

            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var channel = _persistentConnection.CreateModel();
            var args = new Dictionary<string, object>();
            args.Add("x-single-active-consumer", true);

            channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: singleActiveConsumer == false ? null : args);

            // BasicQos method uses which to make it possible to limit the number of unacknowledged messages on a channel.
            channel.BasicQos(0, 1, true);
            var consumer = new EventingBasicConsumer(channel);

            BasicGetResult result = channel.BasicGet(queue, false);
            channel.BasicRecover(true);
            consumer.Received += (ch, ea) =>
            {
                ReceivedEvent(ch, ea, channel);
            };

            consumer.Shutdown += (o, e) =>
            {
                //logging
            };

            channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
        }

        private void ReceivedEvent(object sender, BasicDeliverEventArgs e, IModel channel)
        {
            string customRetryHeaderName = "number-of-retries";
            int retryCount = HelperFunctions.GetRetryCount(e.BasicProperties, customRetryHeaderName);
            var message = Encoding.UTF8.GetString(e.Body.Span);

            try
            {
                var data = JsonConvert.DeserializeObject<dynamic>(message);
                var response = new HttpResponseMessage();

                if (e.RoutingKey == $"{EventBusConstants.DirectQueue}")
                {
                    //var request = JsonConvert.DeserializeObject<>(message);
                }
            }
            catch (Exception ex)
            {
                if (retryCount != 3)
                {
                    IBasicProperties propertiesForCopy = channel.CreateBasicProperties();
                    IDictionary<string, object> headersCopy = HelperFunctions.CopyHeaders(e.BasicProperties);
                    propertiesForCopy.Headers = headersCopy;
                    propertiesForCopy.Headers[customRetryHeaderName] = ++retryCount;
                    channel.BasicPublish(e.Exchange, e.RoutingKey, propertiesForCopy, e.Body);
                }
                else
                {
                    // logging
                }
            }
            finally
            {
                channel.BasicAck(e.DeliveryTag, false);
            }
        }

        public void Disconnect()
        {
            _persistentConnection.Dispose();
        }
    }
}