using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;

namespace WalletService.Infrastructure.Persistence
{
    public class WalletDbContext : DbContext
    {
        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }


        public DbSet<UserWallet> UserWallets => Set<UserWallet>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserWallet>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Address).IsUnique();
                e.Property(x => x.Address).IsRequired().HasMaxLength(256);
                e.Property(x => x.PrivateKey).IsRequired();
                e.Property(x => x.LockedBalance).IsRequired().HasPrecision(38, 18);
            });
        }

    }
}
