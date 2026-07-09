using Microsoft.EntityFrameworkCore;
using TradingService.Domain.Entities;

namespace TradingService.Infrastructure.Persistence
{
    public sealed class TradingDbContext : DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Trade> Trades => Set<Trade>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.PairSymbol);
                entity.HasIndex(x => x.Status);

                entity.Property(x => x.PairSymbol)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Price)
                    .HasPrecision(28, 12);

                entity.Property(x => x.Quantity)
                    .HasPrecision(28, 12);

                entity.Property(x => x.FilledQuantity)
                    .HasPrecision(28, 12);

                entity.Property(x => x.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.DeletedBy)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Trade>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.PairSymbol);
                entity.HasIndex(x => x.BuyOrderId);
                entity.HasIndex(x => x.SellOrderId);

                entity.Property(x => x.PairSymbol)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Price)
                    .HasPrecision(28, 12);

                entity.Property(x => x.Quantity)
                    .HasPrecision(28, 12);

                entity.Property(x => x.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.DeletedBy)
                    .IsRequired()
                    .HasMaxLength(100);
            });
        }
    }
}
