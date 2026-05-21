using AuthService.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.EventDriven;

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
