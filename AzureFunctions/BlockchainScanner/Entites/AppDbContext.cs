using Microsoft.EntityFrameworkCore;

namespace BlockchainScanner.Entites
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<RegisteredWallet> RegisteredWallets => Set<RegisteredWallet>();

        public DbSet<BlockchainTransaction> BlockchainTransactions => Set<BlockchainTransaction>();

        public DbSet<BlockChainState> BlockChainStates => Set<BlockChainState>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BlockChainState>()
                .HasKey(x => x.Network);

            builder.Entity<BlockchainTransaction>()
                .HasIndex(x => x.TransactionHash)
                .IsUnique();
        }
    }
}
