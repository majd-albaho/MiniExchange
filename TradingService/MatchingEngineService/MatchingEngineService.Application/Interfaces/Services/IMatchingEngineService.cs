using MatchingEngineService.Application.Dto;
using MatchingEngineService.Domain;

namespace MatchingEngineService.Application.Interfaces.Services
{
    public interface IMatchingEngineService
    {
        Task<bool> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken cancellationToken = default);

        Task<bool> CancelOrderAsync(Guid orderId, string pairSymbol, CancellationToken cancellationToken = default);

        Task<OrderBookSnapshot> GetOrderBookAsync(string pairSymbol, int depth, CancellationToken cancellationToken = default);
    }
}
