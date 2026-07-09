namespace MatchingEngineService.Domain.Entities
{
    public sealed class BookOrder
    {
        public required Guid Id { get; init; }

        public required string PairSymbol { get; init; }

        public required OrderSide Side { get; init; }

        public required OrderType Type { get; init; }

        public required decimal Price { get; init; }

        public required decimal RemainingQuantity { get; set; }

        public required DateTimeOffset CreatedDate { get; init; }
    }
}
