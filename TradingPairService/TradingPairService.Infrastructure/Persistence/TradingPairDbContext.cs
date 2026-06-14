using Microsoft.EntityFrameworkCore;
using TradingPairService.Domain.Entities;

namespace TradingPairService.Infrastructure.Persistence;

public class TradingPairDbContext : DbContext
{
    public TradingPairDbContext(DbContextOptions<TradingPairDbContext> options) : base(options)
    {
    }

    public DbSet<TradingPair> TradingPairs => Set<TradingPair>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TradingPair>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.Symbol)
                .IsUnique();

            entity.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.BaseAsset)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.QuoteAsset)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(x => x.MinOrderQuantity)
                .HasPrecision(28, 12);

            entity.Property(x => x.MinOrderValue)
                .HasPrecision(28, 12);
        });
    }
}