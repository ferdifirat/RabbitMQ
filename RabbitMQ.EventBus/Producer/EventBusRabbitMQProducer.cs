using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text;

namespace RabbitMQ.EventBus.Producer
{
    public class EventBusRabbitMQProducer
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;

        public EventBusRabbitMQProducer(IRabbitMQPersistentConnection persistentConnection)
        {
            _persistentConnection = persistentConnection;
        }

        public void Publish(string queueName, string message = null, int retryCount = 1)
        {
            if (!_persistentConnection.IsConnected)
            {
                try
                {
                    _persistentConnection.TryConnect();
                }
                catch (Exception ex)
                {
                    //logging
                    return;
                }
            }

            var policy = RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    //logging
                });

            using (var channel = _persistentConnection.CreateModel())
            {
                //Single active consumer allows to have only one consumer at a time consuming from a queue and to fail over to another registered consumer in case the active one is cancelled or dies.
                var args = new Dictionary<string, object>();
                args.Add("x-single-active-consumer", true);

                channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);

                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    IBasicProperties properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.DeliveryMode = 2;

                    channel.ConfirmSelect();
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: queueName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);
                    channel.WaitForConfirmsOrDie();

                    channel.BasicAcks += (sender, eventArgs) =>
                    {
                        //Console.WriteLine("Sent RabbitMq");
                    };
                });
            }
        }
    }
}