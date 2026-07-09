using SharedLibrary.EventDriven.Models;

namespace TradingService.Application.Interfaces.Services
{
    public interface ITradeSettlementService
    {
        Task ApplyTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default);
    }
}
