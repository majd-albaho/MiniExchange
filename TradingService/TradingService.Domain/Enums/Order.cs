using SharedLibrary.Entities;

namespace TradingService.Domain.Entities
{
    public class Order : EntityBase<Guid>
    {
        public Guid UserId { get; set; }

        public required string PairSymbol { get; set; }

        public OrderSide Side { get; set; }

        public OrderType Type { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public decimal FilledQuantity { get; set; }

        public OrderStatus Status { get; set; }
    }
}
