namespace WalletService.Application.Models
{
    /// <summary>
    /// Filter/paging options for a user's transaction history. Type uses the frontend
    /// vocabulary (all/receive/send/buy/sell) and is translated to ledger entry types.
    /// </summary>
    public class WalletTransactionHistoryQuery
    {
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? Symbol { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
