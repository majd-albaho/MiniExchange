using TradingService.Application.Dto;

namespace TradingService.Application.Interfaces.Clients
{
    public interface ITradingPairClient
    {
        Task<TradingPairInfo?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
