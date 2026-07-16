using TradingService.Application.Dto;
using TradingService.Domain.Entities;

namespace TradingService.Application.Interfaces.Repositories
{
    public interface ITradeRepository
    {
        Task<Trade> CreateAsync(Trade trade, CancellationToken cancellationToken = default);

        /// <summary>
        /// Newest-first page of trades the user took part in, each tagged with the side (Buy/Sell)
        /// the user was on. One row per trade, unlike the two-legged wallet ledger.
        /// </summary>
        Task<TradeHistoryPage> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
