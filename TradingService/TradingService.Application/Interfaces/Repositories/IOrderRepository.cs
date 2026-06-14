using TradingService.Domain.Entities;

namespace TradingService.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    }
}
