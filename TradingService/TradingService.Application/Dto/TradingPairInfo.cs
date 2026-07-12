namespace TradingService.Application.Dto
{
    public sealed class TradingPairInfo
    {
        public required string Symbol { get; set; }
        public required string BaseAsset { get; set; }
        public required string QuoteAsset { get; set; }
        public decimal MinOrderQuantity { get; set; }
        public decimal MinOrderValue { get; set; }
        public int PricePrecision { get; set; }
        public int QuantityPrecision { get; set; }
        public bool IsActive { get; set; }
    }
}
