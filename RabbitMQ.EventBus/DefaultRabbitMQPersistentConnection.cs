using Netlog.Crm.EventBus.Helper;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace RabbitMQ.EventBus
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private readonly int _retryCount;
        private bool _disposed;

        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory,
                                                   int retryCount)
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }
        public bool TryConnect()
        {
            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                  {
                      //logging
                  });
            policy.Execute(() =>
            {
                _connection = _connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                return true;
            }
            else
            {
                //logging
                return false;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            this.OnError(EnumEventBusErrorMessage.ConnectionBlocked, EnumErrorLogType.EventBusWarning);
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            this.OnError(EnumEventBusErrorMessage.ConnectionClosed, EnumErrorLogType.EventBusWarning);
            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;
            this.OnError(EnumEventBusErrorMessage.ConnectionClosed, EnumErrorLogType.EventBusWarning);
            TryConnect();
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                this.OnError(EnumEventBusErrorMessage.NoValidConnectionToCreateModel, EnumErrorLogType.EventBusBrokerError);
                throw new InvalidOperationException("No RabbitMQ connections are available to perfrom this action.");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                this.OnError(EnumEventBusErrorMessage.TheResourcesUsedDeleted, EnumErrorLogType.EventBusBrokerError, ex);
            }
        }

        private void OnError(EnumEventBusErrorMessage error, EnumErrorLogType enumEventBusErrorType, Exception ex = null)
        {
            _errorLogRepository.Add(new ErrorLog()
            {
                CreatedOn = DateTime.UtcNow,
                Code = enumEventBusErrorType.ToString(),
                Exception = ex != null ? JsonConvert.SerializeObject(ex) : null,
                ExceptionMessage = error.message,
                ActionDate = DateTime.UtcNow,
                ControllerName = "DefaultRabbitMQPersistentConnection",
                InnerExceptionMessage = ex == null ? String.Empty : JsonConvert.SerializeObject(ex.InnerException),
            });
        }
    }
}