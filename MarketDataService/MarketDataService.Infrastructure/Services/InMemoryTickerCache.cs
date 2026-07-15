using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Application.Models;
using MarketDataService.Domain.Helpers;
using System.Collections.Concurrent;

namespace MarketDataService.Infrastructure.Services
{
    public sealed class InMemoryTickerCache : ITickerCache
    {
        private readonly ConcurrentDictionary<string, MarketTicker> _tickers = new(StringComparer.OrdinalIgnoreCase);

        public MarketTicker? Get(string symbol)
        {
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return null;
            }

            return _tickers.GetValueOrDefault(normalizedSymbol);
        }

        public IReadOnlyCollection<MarketTicker> GetAll()
        {
            return _tickers.Values.ToArray();
        }

        public void Set(MarketTicker ticker)
        {
            _tickers[ticker.Symbol] = ticker;
        }
    }
}
