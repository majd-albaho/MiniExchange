using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Infrastructure.ExternalServices;
using WalletService.Infrastructure.Persistence;
using WalletService.Infrastructure.Repositories;

namespace WalletService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
            services.AddDbContext<WalletDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly("WalletService.SqlMigration")));

            services.AddScoped<IUserWalletRepository, UserWalletRepository>();
            services.AddScoped<IUserWalletAssetsRepository, UserWalletAssetsRepository>();
            services.AddScoped<IUserWalletAddressRepository, UserWalletAddressRepository>();
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            services.AddScoped<ITradeSettlementRepository, TradeSettlementRepository>();

            var marketDataBaseUrl = configuration["MarketDataService:BaseUrl"] ?? "http://localhost:5205";
            services.AddHttpClient<IMarketPriceClient, MarketPriceHttpClient>(client =>
            {
                client.BaseAddress = new Uri(marketDataBaseUrl);
            });

            return services;
        }
    }
}
