using TradingService.Application.Dto;

namespace TradingService.Application.Interfaces.Services
{
    /// <summary>Pushes order/balance change notifications to a specific user's live connections.</summary>
    public interface IOrderNotifier
    {
        Task OrderUpdatedAsync(Guid userId, OrderUpdateNotification notification, CancellationToken cancellationToken = default);
    }
}
