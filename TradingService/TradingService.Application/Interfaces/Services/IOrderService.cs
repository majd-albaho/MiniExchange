using TradingService.Application.Dto;

namespace TradingService.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    }
}
