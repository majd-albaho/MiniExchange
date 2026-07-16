using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Application.Services
{
    public sealed class TradeHistoryService : ITradeHistoryService
    {
        private const int MaxPageSize = 200;

        private readonly ITradeRepository _trades;

        public TradeHistoryService(ITradeRepository trades)
        {
            _trades = trades;
        }

        public async Task<TradeHistoryResponse> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            var normalizedPage = Math.Max(1, page);
            var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            var result = await _trades.GetByUserAsync(userId, normalizedPage, normalizedPageSize, cancellationToken);

            return new TradeHistoryResponse
            {
                Items = result.Items.ToList(),
                Total = result.Total,
                Page = normalizedPage,
                PageSize = normalizedPageSize
            };
        }
    }
}
