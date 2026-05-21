using TradingPairService.Domain.Entities;

namespace TradingPairService.Application.Interfaces.Repositories
{
    public interface ITradingPairRepository
    {
        Task<List<TradingPair>> GetAll();
        Task<TradingPair?> GetBySymbol(string symbol);
        Task<bool> Exists(string symbol);
        Task Add(TradingPair pair);
        Task Update(TradingPair pair);
    }
}
