namespace SharedLibrary.EventDriven.Models
{
    public class TradeExecutedEvent
    {
        public Guid TradeId { get; set; }

        public string PairSymbol { get; set; } = string.Empty;

        public Guid BuyOrderId { get; set; }

        public Guid SellOrderId { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public DateTimeOffset ExecutedAt { get; set; }

        public decimal BuyOrderRemainingQuantity { get; set; }

        public decimal SellOrderRemainingQuantity { get; set; }
    }
}
