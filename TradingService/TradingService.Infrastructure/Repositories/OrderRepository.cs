using Microsoft.EntityFrameworkCore;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Domain.Entities;
using TradingService.Infrastructure.Persistence;

namespace TradingService.Infrastructure.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly TradingDbContext _context;

        public OrderRepository(TradingDbContext context)
        {
            _context = context;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _context.Orders.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedDate == default, cancellationToken);
        }

        public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return order;
        }

        public async Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.DeletedDate == default, cancellationToken);
            if (order is null)
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            order.Status = OrderStatus.Canceled;
            order.DeletedDate = now;
            order.DeletedBy = deletedBy;
            order.ModifiedDate = now;
            order.ModifiedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
