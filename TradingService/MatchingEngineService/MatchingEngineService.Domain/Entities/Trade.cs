namespace MatchingEngineService.Domain.Entities
{
    public sealed class Trade
    {
        public required Guid Id { get; init; }

        public required string PairSymbol { get; init; }

        public required Guid BuyOrderId { get; init; }

        public required Guid SellOrderId { get; init; }

        public required decimal Price { get; init; }

        public required decimal Quantity { get; init; }

        public required DateTimeOffset ExecutedAt { get; init; }

        public required decimal BuyOrderRemainingQuantity { get; init; }

        public required decimal SellOrderRemainingQuantity { get; init; }
    }
}
