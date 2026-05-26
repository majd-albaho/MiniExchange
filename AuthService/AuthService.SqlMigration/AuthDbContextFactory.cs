using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.SqlMigration
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=MiniExchangeWallet;Trusted_Connection=True;TrustServerCertificate=True;",
                sql => sql.MigrationsAssembly("WalletService.SqlMigration"));

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}
