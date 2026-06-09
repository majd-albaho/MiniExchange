using Microsoft.EntityFrameworkCore;
using System.Numerics;
using WalletService.Domain.Entities;

namespace WalletService.Infrastructure.Persistence
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }


        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<UserWallet> UserWallets => Set<UserWallet>();
        public DbSet<UserWalletAddress> UserWalletAddresses => Set<UserWalletAddress>();
        public DbSet<UserWalletAsset> UserWalletAssets => Set<UserWalletAsset>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var bigIntegerConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<BigInteger, decimal>(
                value => (decimal)value,
                value => new BigInteger(value));
        }

    }
}
