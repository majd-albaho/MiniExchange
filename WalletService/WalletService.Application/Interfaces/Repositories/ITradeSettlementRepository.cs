using WalletService.Application.Models;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface ITradeSettlementRepository
    {
        /// <summary>
        /// Atomically moves locked/available balances for both sides of a trade and records the
        /// matching ledger entries. Idempotent on <see cref="TradeSettlementCommand.TradeId"/> —
        /// returns true without re-applying anything if the trade was already settled.
        /// </summary>
        Task<bool> SettleTradeAsync(TradeSettlementCommand command, CancellationToken cancellationToken = default);
    }
}
