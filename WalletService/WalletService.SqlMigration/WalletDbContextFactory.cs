using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WalletService.Infrastructure.Persistence;

namespace WalletService.SqlMigration
{
    public class WalletDbContextFactory : IDesignTimeDbContextFactory<WalletDbContext>
    {
        public WalletDbContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<WalletDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=MiniExchangeWallet;Trusted_Connection=True;TrustServerCertificate=True;",
                sql => sql.MigrationsAssembly("WalletService.SqlMigration"));

            return new WalletDbContext(optionsBuilder.Options);
        }
    }
}
