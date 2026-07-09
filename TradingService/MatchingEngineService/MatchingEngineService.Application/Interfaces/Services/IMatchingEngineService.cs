using MatchingEngineService.Application.Dto;

namespace MatchingEngineService.Application.Interfaces.Services
{
    public interface IMatchingEngineService
    {
        Task<bool> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken cancellationToken = default);

        Task<bool> CancelOrderAsync(Guid orderId, string pairSymbol, CancellationToken cancellationToken = default);
    }
}
