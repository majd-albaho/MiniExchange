using TradingService.Application.Dto;

namespace TradingService.Application.Interfaces.Services
{
    public interface IOrderBookService
    {
        Task<OrderBookResponse> GetOrderBookAsync(string pairSymbol, int depth, CancellationToken cancellationToken = default);
    }
}
