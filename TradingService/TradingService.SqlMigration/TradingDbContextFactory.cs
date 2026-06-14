using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TradingService.Infrastructure.Persistence;

namespace TradingService.SqlMigration
{
    public sealed class TradingDbContextFactory : IDesignTimeDbContextFactory<TradingDbContext>
    {
        public TradingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TradingDbContext>();
            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=MiniExchangeTrading;Trusted_Connection=True;TrustServerCertificate=True;",
                sql => sql.MigrationsAssembly("TradingService.SqlMigration"));

            return new TradingDbContext(optionsBuilder.Options);
        }
    }
}
