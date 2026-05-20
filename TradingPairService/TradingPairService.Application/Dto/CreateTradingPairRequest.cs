namespace TradingPairService.Application.Dto
{
    public class CreateTradingPairRequest
    {
        public string BaseAsset { get; set; } = string.Empty;
        public string QuoteAsset { get; set; } = string.Empty;
        public decimal MinOrderQuantity { get; set; }
        public decimal MinOrderValue { get; set; }
        public int PricePrecision { get; set; }
        public int QuantityPrecision { get; set; }
    }
}
