using Microsoft.Extensions.DependencyInjection;
using TradingService.Application.Interfaces.Services;
using TradingService.Application.Services;

namespace TradingService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();
            return services;
        }
    }
}
