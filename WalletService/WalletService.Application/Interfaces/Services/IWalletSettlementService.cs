using WalletService.Application.Models;

namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletSettlementService
    {
        Task<bool> SettleTradeAsync(TradeSettlementRequest request, CancellationToken cancellationToken = default);
    }
}
