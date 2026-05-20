using SharedLibrary.Entities;

namespace TradingPairService.Domain.Entities
{
    public class TradingPair : EntityBase<Guid>
    {
        public string Symbol { get; set; } = string.Empty;

        public string BaseAsset { get; set; } = string.Empty;

        public string QuoteAsset { get; set; } = string.Empty;

        public decimal MinOrderQuantity { get; set; }

        public decimal MinOrderValue { get; set; }

        public int PricePrecision { get; set; }

        public int QuantityPrecision { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
