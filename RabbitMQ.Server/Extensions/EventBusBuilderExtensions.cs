using RabbitMQ.EventBus.Core;
using RabbitMQ.Server.Messaging;
using RabbitMQ.Server.Messaging.Consumers;

namespace RabbitMQ.Server.Extensions
{
    public static class EventBusBuilderExtensions
    {
        public static IServiceProvider _serviceProvider;
        public static RpcServer RpcListener { get;set; }

        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var life = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            life.ApplicationStarted.Register(OnStarted);
            life.ApplicationStopped.Register(OnStopping);
            RpcListener = app.ApplicationServices.GetService<RpcServer>();

            return app;
        }

        private static void OnStarted()
        {
            ActivatorUtilities.CreateInstance<Consumer>(_serviceProvider).Consume($"{EventBusConstants.DirectQueue}");
            RpcListener.Consume($"{EventBusConstants.RdcPublishQueue}");
        }
        private static void OnStopping()
        {

        }
    }
}
