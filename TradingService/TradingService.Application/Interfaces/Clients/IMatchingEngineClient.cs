using TradingService.Application.Dto;
using TradingService.Domain.Entities;

namespace TradingService.Application.Interfaces.Clients
{
    public interface IMatchingEngineClient
    {
        Task<bool> SubmitOrderAsync(Order order, CancellationToken cancellationToken = default);
        Task<bool> CancelOrderAsync(Guid orderId, string pairSymbol, CancellationToken cancellationToken = default);
        Task<OrderBookSnapshotDto> GetOrderBookAsync(string pairSymbol, int depth, CancellationToken cancellationToken = default);
    }
}
