using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TradingPairService.Infrastructure.Persistence;

namespace TradingPairService.SqlMigration;

public sealed class TradingPairDbContextFactory : IDesignTimeDbContextFactory<TradingPairDbContext>
{
    public TradingPairDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TradingPairDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=MiniExchangeTradingPair;Trusted_Connection=True;TrustServerCertificate=True;",
            sql => sql.MigrationsAssembly("TradingPairService.SqlMigration"));

        return new TradingPairDbContext(optionsBuilder.Options);
    }
}
