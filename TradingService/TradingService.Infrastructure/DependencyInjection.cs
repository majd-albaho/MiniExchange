using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Infrastructure.Persistence;
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
            return services;
        }
    }
}
