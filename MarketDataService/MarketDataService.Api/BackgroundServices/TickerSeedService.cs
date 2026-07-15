using MarketDataService.Application.Interfaces.Services;

namespace MarketDataService.Api.BackgroundServices
{
    /// <summary>
    /// Subscribes to a configured default set of symbols on startup so ticker/price data is
    /// available (home page, header strip) without a user first opening the trade page.
    /// </summary>
    public sealed class TickerSeedService : BackgroundService
    {
        private static readonly string[] FallbackSymbols = ["BTCUSDT", "ETHUSDT", "SOLUSDT", "BNBUSDT"];

        private readonly ISubscriptionService _subscriptionService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TickerSeedService> _logger;

        public TickerSeedService(ISubscriptionService subscriptionService, IConfiguration configuration, ILogger<TickerSeedService> logger)
        {
            _subscriptionService = subscriptionService;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var symbols = _configuration.GetSection("MarketData:DefaultSymbols").Get<string[]>();
            if (symbols is null || symbols.Length == 0)
            {
                symbols = FallbackSymbols;
            }

            foreach (var symbol in symbols)
            {
                try
                {
                    await _subscriptionService.SubscribeAsync(symbol, stoppingToken);
                    _logger.LogInformation("Seeded default ticker subscription for {Symbol}", symbol);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to seed default ticker subscription for {Symbol}", symbol);
                }
            }
        }
    }
}
