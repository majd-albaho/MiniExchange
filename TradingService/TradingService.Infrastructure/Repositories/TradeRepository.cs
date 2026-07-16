using Microsoft.EntityFrameworkCore;
using TradingService.Application.Dto;
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

        public async Task<TradeHistoryPage> GetByUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            // Join each trade to its buy and sell orders to find the user's side. Orders are joined
            // regardless of soft-delete so trades from a later-cancelled partial fill still show.
            var query =
                from t in _context.Trades
                join buy in _context.Orders on t.BuyOrderId equals buy.Id
                join sell in _context.Orders on t.SellOrderId equals sell.Id
                where t.DeletedDate == default && (buy.UserId == userId || sell.UserId == userId)
                orderby t.ExecutedAt descending, t.Id descending
                select new
                {
                    t.Id,
                    t.PairSymbol,
                    t.Price,
                    t.Quantity,
                    t.ExecutedAt,
                    IsBuyer = buy.UserId == userId
                };

            var total = await query.CountAsync(cancellationToken);

            var rows = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows
                .Select(r => new TradeHistoryItem
                {
                    TradeId = r.Id,
                    PairSymbol = r.PairSymbol,
                    Side = r.IsBuyer ? OrderSide.Buy : OrderSide.Sell,
                    Price = r.Price,
                    Quantity = r.Quantity,
                    QuoteAmount = r.Price * r.Quantity,
                    ExecutedAt = r.ExecutedAt
                })
                .ToList();

            return new TradeHistoryPage { Items = items, Total = total };
        }
    }
}
