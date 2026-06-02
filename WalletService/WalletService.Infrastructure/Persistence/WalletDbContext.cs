using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;

namespace WalletService.Infrastructure.Persistence
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }


        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<UserWallet> UserWallets => Set<UserWallet>();
        public DbSet<UserWalletAsset> UserWalletAssets => Set<UserWalletAsset>();
        public DbSet<Wallet> Wallets => Set<Wallet>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

    }
}
