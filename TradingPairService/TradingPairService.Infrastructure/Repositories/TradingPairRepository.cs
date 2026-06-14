using Microsoft.EntityFrameworkCore;
using TradingPairService.Application.Interfaces.Repositories;
using TradingPairService.Domain.Entities;
using TradingPairService.Infrastructure.Persistence;

namespace TradingPairService.Infrastructure.Repositories
{
    public class TradingPairRepository : ITradingPairRepository
    {
        private readonly TradingPairDbContext _context;

        public TradingPairRepository(TradingPairDbContext context)
        {
            _context = context;
        }

        public Task<List<TradingPair>> GetAll()
        {
            return _context.TradingPairs.AsNoTracking()
                .OrderBy(x => x.Symbol)
                .ToListAsync();
        }

        public Task<TradingPair?> GetBySymbol(string symbol)
        {
            return _context.TradingPairs.FirstOrDefaultAsync(x => x.Symbol == symbol);
        }

        public Task<bool> Exists(string symbol)
        {
            return _context.TradingPairs.AnyAsync(x => x.Symbol == symbol);
        }

        public async Task Add(TradingPair pair)
        {
            await _context.TradingPairs.AddAsync(pair);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TradingPair pair)
        {
            _context.TradingPairs.Update(pair);
            await _context.SaveChangesAsync();
        }
    }
}
