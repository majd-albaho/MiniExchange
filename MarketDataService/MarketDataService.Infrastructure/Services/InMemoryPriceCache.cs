using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Application.Models;
using MarketDataService.Domain.Helpers;
using System.Collections.Concurrent;

namespace MarketDataService.Infrastructure.Services
{
    public sealed class InMemoryPriceCache : IPriceCache
    {
        private readonly ConcurrentDictionary<string, CryptoCurrencyPrice> _prices = new(StringComparer.OrdinalIgnoreCase);

        public CryptoCurrencyPrice? Get(string symbol)
        {
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return null;
            }

            return _prices.GetValueOrDefault(normalizedSymbol);
        }

        public void Set(CryptoCurrencyPrice price)
        {
            _prices[price.Symbol] = price;
        }
    }
}
