using WalletService.Domain.Enums;

namespace WalletService.Application.Models
{
    /// <summary>A ledger row joined with its asset name, as returned by the repository page query.</summary>
    public class WalletTransactionHistoryEntry
    {
        public long Id { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ExternalReference { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class WalletTransactionHistoryPage
    {
        public IReadOnlyList<WalletTransactionHistoryEntry> Items { get; set; } = [];
        public int Total { get; set; }
    }
}
