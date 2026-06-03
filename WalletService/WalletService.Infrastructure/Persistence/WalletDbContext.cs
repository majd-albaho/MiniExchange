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
        public DbSet<UserWalletTransaction> UserWalletTransactions => Set<UserWalletTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var bigIntegerConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<BigInteger, decimal>(
                value => (decimal)value,
                value => new BigInteger(value));

            modelBuilder.Entity<UserWalletTransaction>(entity =>
            {
                entity.Property(transaction => transaction.BlockNumber)
                    .HasConversion(bigIntegerConverter)
                    .HasColumnType("decimal(38, 0)");

                entity.Property(transaction => transaction.GasUsed)
                    .HasConversion(bigIntegerConverter)
                    .HasColumnType("decimal(38, 0)");

                entity.Property(transaction => transaction.EffectiveGasPrice)
                    .HasConversion(bigIntegerConverter)
                    .HasColumnType("decimal(38, 0)");
            });
        }

    }
}
