using System.Net;
using System.Text.Json;
using TradingService.Application.Interfaces.Clients;

namespace TradingService.Infrastructure.HttpClients
{
    public sealed class MarketDataHttpClient : IMarketDataClient
    {
        private readonly HttpClient _httpClient;

        public MarketDataHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal?> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"api/Markets/price/{Uri.EscapeDataString(symbol)}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            return document.RootElement.GetProperty("lastPrice").GetDecimal();
        }
    }
}
