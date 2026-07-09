using MatchingEngineService.Domain.Entities;

namespace MatchingEngineService.Application.Dto
{
    public sealed class SubmitOrderCommand
    {
        public required Guid OrderId { get; init; }

        public required string PairSymbol { get; init; }

        public required OrderSide Side { get; init; }

        public required OrderType Type { get; init; }

        public required decimal Price { get; init; }

        public required decimal Quantity { get; init; }
    }
}
