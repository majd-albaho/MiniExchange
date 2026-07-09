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

        public async Task<bool> UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            var tracked = await _context.Orders.FirstOrDefaultAsync(x => x.Id == order.Id && x.DeletedDate == default, cancellationToken);
            if (tracked is null)
            {
                return false;
            }

            tracked.FilledQuantity = order.FilledQuantity;
            tracked.Status = order.Status;
            tracked.ModifiedDate = DateTimeOffset.UtcNow;
            tracked.ModifiedBy = order.ModifiedBy;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IReadOnlyList<Order>> GetOpenByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders.AsNoTracking()
                .Where(x => x.UserId == userId && x.DeletedDate == default
                    && (x.Status == OrderStatus.Pending || x.Status == OrderStatus.PartiallyFilled))
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }
    }
}
