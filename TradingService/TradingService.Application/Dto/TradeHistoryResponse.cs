using TradingService.Domain.Entities;

namespace TradingService.Application.Dto
{
    /// <summary>A single trade seen from one user's perspective (their side of the fill).</summary>
    public sealed class TradeHistoryItem
    {
        public Guid TradeId { get; set; }
        public required string PairSymbol { get; set; }
        public OrderSide Side { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuoteAmount { get; set; }
        public DateTimeOffset ExecutedAt { get; set; }
    }

    /// <summary>Repository-level page of a user's trades (entity + resolved side).</summary>
    public sealed class TradeHistoryPage
    {
        public IReadOnlyList<TradeHistoryItem> Items { get; set; } = [];
        public int Total { get; set; }
    }

    public sealed class TradeHistoryResponse
    {
        public List<TradeHistoryItem> Items { get; set; } = [];
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
