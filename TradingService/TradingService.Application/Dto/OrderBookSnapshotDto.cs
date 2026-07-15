namespace TradingService.Application.Dto
{
    /// <summary>Aggregated depth as returned by the matching engine (quantity resting per price).</summary>
    public sealed class OrderBookLevelDto
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
    }

    public sealed class OrderBookSnapshotDto
    {
        public required string PairSymbol { get; set; }
        public IReadOnlyList<OrderBookLevelDto> Bids { get; set; } = [];
        public IReadOnlyList<OrderBookLevelDto> Asks { get; set; } = [];
    }
}
