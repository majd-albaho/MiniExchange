using AuthService.Application.Interfaces.Services;
using AuthService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, Services.AuthService>();
            services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
            return services;
        }
    }
}
