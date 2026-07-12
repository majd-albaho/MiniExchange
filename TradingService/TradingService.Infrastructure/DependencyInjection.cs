using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingService.Application.Interfaces.Clients;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Infrastructure.GrpcClients;
using TradingService.Infrastructure.HttpClients;
using TradingService.Infrastructure.Persistence;
using TradingService.Infrastructure.Protos;
using TradingService.Infrastructure.Repositories;

namespace TradingService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TradingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly("TradingService.SqlMigration")));

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ITradeRepository, TradeRepository>();

            var matchingEngineAddress = configuration["MatchingEngineService:GrpcAddress"] ?? "http://localhost:5210";
            services.AddGrpcClient<MatchingEngineGrpc.MatchingEngineGrpcClient>(options =>
            {
                options.Address = new Uri(matchingEngineAddress);
            });
            services.AddScoped<IMatchingEngineClient, MatchingEngineGrpcClient>();

            var tradingPairAddress = configuration["TradingPairService:GrpcAddress"] ?? "http://localhost:5208";
            services.AddGrpcClient<TradingPairGrpc.TradingPairGrpcClient>(options =>
            {
                options.Address = new Uri(tradingPairAddress);
            });
            services.AddScoped<ITradingPairClient, TradingPairGrpcClient>();

            var walletServiceAddress = configuration["WalletService:GrpcAddress"] ?? "http://localhost:5206";
            services.AddGrpcClient<WalletGrpc.WalletGrpcClient>(options =>
            {
                options.Address = new Uri(walletServiceAddress);
            });
            services.AddScoped<IWalletServiceClient, WalletServiceGrpcClient>();

            var marketDataAddress = configuration["MarketDataService:BaseUrl"] ?? "http://localhost:5205";
            services.AddHttpClient<IMarketDataClient, MarketDataHttpClient>(client =>
            {
                client.BaseAddress = new Uri(marketDataAddress);
            });

            return services;
        }
    }
}
