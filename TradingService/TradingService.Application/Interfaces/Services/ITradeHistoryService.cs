using TradingService.Application.Dto;

namespace TradingService.Application.Interfaces.Services
{
    public interface ITradeHistoryService
    {
        Task<TradeHistoryResponse> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
