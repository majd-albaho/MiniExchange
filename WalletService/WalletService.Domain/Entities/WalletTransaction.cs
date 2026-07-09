using SharedLibrary.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Domain.Entities
{
    /// <summary>
    /// Ledger record of a single balance movement (dev credit, or one side of a trade settlement).
    /// Distinct from <see cref="UserWalletAsset"/>, which only holds the current running totals.
    /// </summary>
    public class WalletTransaction : EntityBase<long>
    {
        public required long UserWalletId { get; set; }
        public required long AssetId { get; set; }
        public required WalletTransactionType Type { get; set; }
        public required decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }

        /// <summary>TradeId for TradeDebit/TradeCredit entries; null for a manual Credit.</summary>
        public Guid? ReferenceId { get; set; }
    }
}
