using Microsoft.Extensions.DependencyInjection;
using MatchingEngineService.Application.Interfaces.Services;
using SharedLibrary.EventDriven;

namespace MatchingEngineService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
            services.AddSingleton<OrderBookRegistry>();
            services.AddSingleton<IMatchingEngineService, Services.MatchingEngineService>();
            return services;
        }
    }
}
