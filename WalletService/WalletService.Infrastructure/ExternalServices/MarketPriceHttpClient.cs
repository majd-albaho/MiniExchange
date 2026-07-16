using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Models;

namespace WalletService.Infrastructure.ExternalServices
{
    public sealed class MarketPriceHttpClient : IMarketPriceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MarketPriceHttpClient> _logger;

        public MarketPriceHttpClient(HttpClient httpClient, ILogger<MarketPriceHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IReadOnlyDictionary<string, AssetPrice>> GetPricesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var tickers = await _httpClient.GetFromJsonAsync<List<TickerResponse>>("api/Markets/tickers", cancellationToken)
                    ?? new List<TickerResponse>();

                return tickers
                    .Where(t => !string.IsNullOrWhiteSpace(t.Symbol))
                    .ToDictionary(
                        t => t.Symbol!,
                        t => new AssetPrice(t.LastPrice, t.PriceChangePercent),
                        StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read market prices from MarketDataService; USDT values will be 0.");
                return new Dictionary<string, AssetPrice>();
            }
        }

        private sealed class TickerResponse
        {
            public string? Symbol { get; set; }
            public decimal LastPrice { get; set; }
            public decimal PriceChangePercent { get; set; }
        }
    }
}
