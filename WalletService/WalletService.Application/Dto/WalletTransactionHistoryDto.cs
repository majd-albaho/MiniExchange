namespace WalletService.Application.Dto
{
    /// <summary>One transaction row shaped for the frontend transactions page.</summary>
    public class WalletTransactionHistoryItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal AmountUSDT { get; set; }
        public decimal Fee { get; set; }
        public string FeeSymbol { get; set; } = string.Empty;
        public string? TxHash { get; set; }
        public string? Network { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public class WalletTransactionHistoryResponseDto
    {
        public List<WalletTransactionHistoryItemDto> Items { get; set; } = [];
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
