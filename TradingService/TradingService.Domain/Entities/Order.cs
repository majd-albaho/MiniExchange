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

        /// <summary>WalletService asset id for the pair's base asset, resolved at creation time.</summary>
        public long? BaseAssetId { get; set; }

        /// <summary>WalletService asset id for the pair's quote asset, resolved at creation time.</summary>
        public long? QuoteAssetId { get; set; }

        /// <summary>
        /// Remaining amount still reserved in WalletService for this order (base asset for a Sell,
        /// quote asset for a Buy). Decremented as trades settle so it always reflects exactly what
        /// still needs unlocking on cancel/expiry, even when a taker fills at a better price than
        /// its own limit price.
        /// </summary>
        public decimal LockedAmount { get; set; }
    }
}
