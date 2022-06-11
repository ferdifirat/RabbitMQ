using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Core.Model;
using RabbitMQ.EventBus.Core;
using System.Collections.Concurrent;
using System.Text;

namespace RabbitMQ.EventBus.Producer
{
    public class RpcClient
    {
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        private readonly IRabbitMQPersistentConnection _persistentConnection;

        public RpcClient(IRabbitMQPersistentConnection persistentConnection)
        {
            _persistentConnection = persistentConnection;

            if (!persistentConnection.IsConnected)
                persistentConnection.TryConnect();

            channel = persistentConnection.CreateModel();
            channel.ConfirmSelect();
            replyQueueName = $"{ EventBusConstants.RdcReplyQueue}";

            var args = new Dictionary<string, object>();
            args.Add("x-single-active-consumer", true);

            channel.QueueDeclare(queue: replyQueueName, durable: false,
                  exclusive: false, autoDelete: false, arguments: args);
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            string correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                string response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
            channel.BasicAcks += (sender, ea) =>
            {

            };
            channel.BasicNacks += (sender, ea) =>
            {

            };
        }

        public Response Call<T>(T obj)
        {
            var message = JsonConvert.SerializeObject(obj);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
               exchange: "",
               routingKey: $"{ EventBusConstants.RdcPublishQueue}",
               basicProperties: props,
               body: body);

            channel.BasicConsume(
               consumer: consumer,
               queue: replyQueueName,
               autoAck: true);

            var receivedMessage = respQueue.Take();

            return JsonConvert.DeserializeObject<Response>(receivedMessage);
        }

        public void Close()
        {
            _persistentConnection.Dispose();
        }
    }
}
