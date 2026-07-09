using TradingService.Application.Interfaces.Repositories;
using TradingService.Domain.Entities;
using TradingService.Infrastructure.Persistence;

namespace TradingService.Infrastructure.Repositories
{
    public sealed class TradeRepository : ITradeRepository
    {
        private readonly TradingDbContext _context;

        public TradeRepository(TradingDbContext context)
        {
            _context = context;
        }

        public async Task<Trade> CreateAsync(Trade trade, CancellationToken cancellationToken = default)
        {
            await _context.Trades.AddAsync(trade, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return trade;
        }
    }
}
