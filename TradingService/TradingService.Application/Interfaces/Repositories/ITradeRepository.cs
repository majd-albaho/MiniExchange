using TradingService.Domain.Entities;

namespace TradingService.Application.Interfaces.Repositories
{
    public interface ITradeRepository
    {
        Task<Trade> CreateAsync(Trade trade, CancellationToken cancellationToken = default);
    }
}
