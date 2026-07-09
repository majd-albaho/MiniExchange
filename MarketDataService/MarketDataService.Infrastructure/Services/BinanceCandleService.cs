using System.Globalization;
using System.Text.Json;
using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Application.Models;

namespace MarketDataService.Infrastructure.Services
{
    public sealed class BinanceCandleService : ICandleService
    {
        private readonly HttpClient _httpClient;

        public BinanceCandleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://api.binance.com/");
        }

        public async Task<IReadOnlyList<Candle>> GetCandlesAsync(string symbol, string interval, int limit, CancellationToken cancellationToken = default)
        {
            var url = $"api/v3/klines?symbol={Uri.EscapeDataString(symbol)}&interval={Uri.EscapeDataString(interval)}&limit={limit}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var candles = new List<Candle>();

            foreach (var entry in document.RootElement.EnumerateArray())
            {
                var openTimeMs = entry[0].GetInt64();
                var open = decimal.Parse(entry[1].GetString()!, CultureInfo.InvariantCulture);
                var high = decimal.Parse(entry[2].GetString()!, CultureInfo.InvariantCulture);
                var low = decimal.Parse(entry[3].GetString()!, CultureInfo.InvariantCulture);
                var close = decimal.Parse(entry[4].GetString()!, CultureInfo.InvariantCulture);
                var volume = decimal.Parse(entry[5].GetString()!, CultureInfo.InvariantCulture);

                candles.Add(new Candle(openTimeMs / 1000, open, high, low, close, volume));
            }

            return candles;
        }
    }
}
