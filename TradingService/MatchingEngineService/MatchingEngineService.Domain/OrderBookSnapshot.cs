namespace MatchingEngineService.Domain
{
    /// <summary>One aggregated price level: the total remaining quantity resting at that price.</summary>
    public sealed record OrderBookLevel(decimal Price, decimal Quantity);

    /// <summary>
    /// Point-in-time view of a book's best bids/asks, aggregated per price level.
    /// Bids are ordered best (highest) first, asks best (lowest) first.
    /// </summary>
    public sealed record OrderBookSnapshot(
        string PairSymbol,
        IReadOnlyList<OrderBookLevel> Bids,
        IReadOnlyList<OrderBookLevel> Asks);
}
