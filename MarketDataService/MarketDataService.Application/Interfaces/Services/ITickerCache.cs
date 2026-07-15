using MarketDataService.Application.Models;

namespace MarketDataService.Application.Interfaces.Services
{
    public interface ITickerCache
    {
        MarketTicker? Get(string symbol);

        IReadOnlyCollection<MarketTicker> GetAll();

        void Set(MarketTicker ticker);
    }
}
