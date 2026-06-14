using TradingService.Domain.Entities;

namespace TradingService.Application.Dto
{
    public sealed class CreateOrderRequest
    {
        public Guid UserId { get; set; }
        public required string PairSymbol { get; set; }
        public OrderSide Side { get; set; }
        public OrderType Type { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string? CreatedBy { get; set; }
    }
}
