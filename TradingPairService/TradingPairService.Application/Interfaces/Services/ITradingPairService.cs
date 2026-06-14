using TradingPairService.Application.Dto;

namespace TradingPairService.Application.Interfaces.Services
{
    public interface ITradingPairService
    {
        Task<List<TradingPairResponse>> GetAll();
        Task<TradingPairResponse?> GetBySymbol(string symbol);
        Task<TradingPairResponse> Create(CreateTradingPairRequest request);
        Task Activate(string symbol);
        Task Deactivate(string symbol);
    }
}
