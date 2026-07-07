using Microsoft.Extensions.DependencyInjection;
using TradingPairService.Application.Interfaces.Services;

namespace TradingPairService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ITradingPairService, Services.TradingPairService>();
            return services;
        }
    }
}
