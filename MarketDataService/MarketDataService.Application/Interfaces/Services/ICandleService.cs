using MarketDataService.Application.Models;

namespace MarketDataService.Application.Interfaces.Services
{
    public interface ICandleService
    {
        Task<IReadOnlyList<Candle>> GetCandlesAsync(string symbol, string interval, int limit, CancellationToken cancellationToken = default);
    }
}
