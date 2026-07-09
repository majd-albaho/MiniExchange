namespace WalletService.Application.Models
{
    public sealed class TradeSettlementRequest
    {
        public Guid TradeId { get; set; }
        public Guid BuyerUserId { get; set; }
        public Guid SellerUserId { get; set; }
        public long BaseAssetId { get; set; }
        public long QuoteAssetId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
