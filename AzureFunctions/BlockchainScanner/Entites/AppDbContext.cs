using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace BlockchainScanner.Entites
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RegisteredWallet> RegisteredWallets => Set<RegisteredWallet>();

        public DbSet<BlockchainTransaction> BlockchainTransactions => Set<BlockchainTransaction>();

        public DbSet<BlockChainState> BlockChainStates => Set<BlockChainState>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BlockChainState>()
                .Property(x => x.LastProcessedBlock)
                .HasConversion(x => (decimal)x, x => new BigInteger(x))
                .HasColumnType("decimal(38,0)");

            builder.Entity<BlockchainTransaction>()
                .Property(x => x.BlockNumber)
                .HasConversion(x => (decimal)x, x => new BigInteger(x))
                .HasColumnType("decimal(38,0)");

            builder.Entity<BlockchainTransaction>()
                .Property(x => x.GasUsed)
                .HasConversion(x => (decimal)x, x => new BigInteger(x))
                .HasColumnType("decimal(38,0)");

            builder.Entity<BlockchainTransaction>()
                .Property(x => x.EffectiveGasPrice)
                .HasConversion(x => (decimal)x, x => new BigInteger(x))
                .HasColumnType("decimal(38,0)");

            builder.Entity<BlockchainTransaction>()
                .Property(x => x.Amount)
                .HasColumnType("decimal(38,18)");

            builder.Entity<BlockChainState>()
                .HasKey(x => x.Network);

            builder.Entity<BlockchainTransaction>()
                .HasIndex(x => x.TransactionHash)
                .IsUnique();

            builder.Entity<RegisteredWallet>()
                .HasIndex(x => x.UserId)
                .IsUnique();
        }
    }
}
