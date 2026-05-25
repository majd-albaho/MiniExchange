using Microsoft.Extensions.DependencyInjection;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Services;

namespace WalletService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserWalletService,UserWalletService>();
            return services;
        }
    }
}
