using Microsoft.EntityFrameworkCore;
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
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserWalletAsset>(entity =>
            {
                entity.Property(x => x.Amount).HasPrecision(28, 12);
                entity.Property(x => x.LockedAmount).HasPrecision(28, 12);
            });

            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.HasIndex(x => x.UserWalletId);
                entity.HasIndex(x => x.ReferenceId);

                entity.Property(x => x.Amount).HasPrecision(28, 12);
                entity.Property(x => x.BalanceAfter).HasPrecision(28, 12);
            });
        }
    }
}
