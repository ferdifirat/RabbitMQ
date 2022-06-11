using RabbitMQ.EventBus.Core;
using RabbitMQ.Server.Messaging.Consumers;

namespace RabbitMQ.Server.Extensions
{
    public static class EventBusBuilderExtensions
    {
        public static IServiceProvider _serviceProvider;
        public static IApplicationBuilder UseRabbitListener(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var life = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            life.ApplicationStarted.Register(OnStarted);
            life.ApplicationStopped.Register(OnStopping);
            return app;
        }

        private static void OnStarted()
        {
            ActivatorUtilities.CreateInstance<Consumer>(_serviceProvider).Consume($"{EventBusConstants.DirectQueue}");
        }
        private static void OnStopping()
        {

        }
    }
}
