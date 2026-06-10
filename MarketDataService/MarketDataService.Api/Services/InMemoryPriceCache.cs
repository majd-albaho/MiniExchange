using MarketDataService.Application.Models;
using System.Collections.Concurrent;

namespace MarketDataService.Api.Services
{
    public sealed class InMemoryPriceCache : IPriceCache
    {
        private readonly ConcurrentDictionary<string, BinancePrice> _prices = new(StringComparer.OrdinalIgnoreCase);

        public BinancePrice? Get(string symbol)
        {
            if (!BinanceSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return null;
            }

            return _prices.GetValueOrDefault(normalizedSymbol);
        }

        public void Set(BinancePrice price)
        {
            _prices[price.Symbol] = price;
        }
    }
}
