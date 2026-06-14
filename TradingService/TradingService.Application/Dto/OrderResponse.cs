using TradingService.Domain.Entities;

namespace TradingService.Application.Dto
{
    public sealed class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public required string PairSymbol { get; set; }
        public OrderSide Side { get; set; }
        public OrderType Type { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal FilledQuantity { get; set; }
        public OrderStatus Status { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
