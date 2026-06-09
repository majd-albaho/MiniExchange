using BlockchainScanner;
using BlockchainScanner.Entites;
using BlockchainScanner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlockchainScannerUnitTest
{
    public sealed class BlockchainScannerTests
    {
        [Fact]
        public void InitialMigration_IsDiscoverable()
        {
            using var db = CreateSqlServerContext();
            db.Database.Migrate();

            Assert.Contains(
                db.Database.GetMigrations(),
                migration => migration.EndsWith("_InitialCreate", StringComparison.Ordinal));
        }

        [Fact]
        public void SqlServerModel_MapsBlockchainValuesAndIndexes()
        {
            using var db = CreateSqlServerContext();

            var state = db.Model.FindEntityType(typeof(BlockChainState));
            Assert.NotNull(state);
            Assert.Equal("decimal(38,0)", state.FindProperty(nameof(BlockChainState.LastProcessedBlock))?.GetColumnType());

            var transaction = db.Model.FindEntityType(typeof(BlockchainTransaction));
            Assert.NotNull(transaction);
            Assert.Equal("decimal(38,0)", transaction.FindProperty(nameof(BlockchainTransaction.BlockNumber))?.GetColumnType());
            Assert.Equal("decimal(38,0)", transaction.FindProperty(nameof(BlockchainTransaction.GasUsed))?.GetColumnType());
            Assert.Equal("decimal(38,0)", transaction.FindProperty(nameof(BlockchainTransaction.EffectiveGasPrice))?.GetColumnType());
            Assert.Equal("decimal(38,18)", transaction.FindProperty(nameof(BlockchainTransaction.Amount))?.GetColumnType());

            Assert.Contains(transaction.GetIndexes(), index =>
                index.IsUnique &&
                index.Properties.Count == 1 &&
                index.Properties[0].Name == nameof(BlockchainTransaction.TransactionHash));

            var wallet = db.Model.FindEntityType(typeof(RegisteredWallet));
            Assert.NotNull(wallet);
            Assert.Contains(wallet.GetIndexes(), index =>
                index.IsUnique &&
                index.Properties.Count == 1 &&
                index.Properties[0].Name == nameof(RegisteredWallet.UserId));
        }

        [Fact]
        public async Task DependencyInjection_ResolvesScannerAndFunction()
        {
            var originalSqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            var originalAlchemyRpcUrl = Environment.GetEnvironmentVariable("AlchemyRpcUrl");

            Environment.SetEnvironmentVariable("SqlConnectionString", TestConnectionString);
            Environment.SetEnvironmentVariable("AlchemyRpcUrl", "https://eth-sepolia.g.alchemy.com/v2/7tloHtXeoED-phvbnG5Fe");

            try
            {
                var services = new ServiceCollection();
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString"));
                });
                services.AddScoped<EthereumScanner>();
                services.AddScoped<ScanEthereumFunction>();

                using var provider = services.BuildServiceProvider(new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
                using var scope = provider.CreateScope();

                using var db = CreateSqlServerContext();
                db.Database.Migrate();

                var scanner = scope.ServiceProvider.GetRequiredService<EthereumScanner>();
                await scanner.ScanAsync();

                Assert.NotNull(scanner);
            }
            finally
            {
                Environment.SetEnvironmentVariable("SqlConnectionString", originalSqlConnectionString);
                Environment.SetEnvironmentVariable("AlchemyRpcUrl", originalAlchemyRpcUrl);
            }
        }

        private static AppDbContext CreateSqlServerContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(TestConnectionString)
                .Options;

            return new AppDbContext(options);
        }

        private const string TestConnectionString =
            "Server=localhost;Database=BlockchainScanner;Trusted_Connection=True;TrustServerCertificate=True";
    }
}
