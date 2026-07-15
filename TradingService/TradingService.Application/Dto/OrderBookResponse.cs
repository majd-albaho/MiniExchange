namespace TradingService.Application.Dto
{
    /// <summary>One order-book row shaped for the frontend: quantity plus its quote-notional total.</summary>
    public sealed class OrderBookEntryResponse
    {
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
    }

    public sealed class OrderBookResponse
    {
        public required string Pair { get; set; }
        public List<OrderBookEntryResponse> Asks { get; set; } = [];
        public List<OrderBookEntryResponse> Bids { get; set; } = [];
        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
