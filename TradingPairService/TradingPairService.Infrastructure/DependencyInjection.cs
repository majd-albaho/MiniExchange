using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingPairService.Application.Interfaces.Repositories;
using TradingPairService.Application.Interfaces.Services;
using TradingPairService.Infrastructure.Persistence;
using TradingPairService.Infrastructure.Repositories;

namespace TradingPairService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TradingPairDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ITradingPairRepository, TradingPairRepository>();
            services.AddScoped<ITradingPairService, Application.Services.TradingPairService>();
            return services;
        }
    }
}
