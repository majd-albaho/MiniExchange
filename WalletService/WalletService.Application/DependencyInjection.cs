using Microsoft.Extensions.DependencyInjection;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Services;

namespace WalletService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IWalletBlockchainClient, NethereumWalletBlockchainClient>();
            services.AddScoped<IUserWalletService, UserWalletService>();
            services.AddScoped<IWalletFundService, WalletFundService>();
            services.AddScoped<IWalletTransactionService, WalletTransactionService>();
            services.AddScoped<IWalletDepositService, WalletDepositService>();
            services.AddScoped<IWalletOverviewService, WalletOverviewService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IWalletSettlementService, WalletSettlementService>();
            return services;
        }
    }
}
