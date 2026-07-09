using SharedLibrary.Entities;

namespace TradingService.Domain.Entities
{
    public class Trade : EntityBase<Guid>
    {
        public required string PairSymbol { get; set; }

        public Guid BuyOrderId { get; set; }

        public Guid SellOrderId { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public DateTimeOffset ExecutedAt { get; set; }
    }
}
