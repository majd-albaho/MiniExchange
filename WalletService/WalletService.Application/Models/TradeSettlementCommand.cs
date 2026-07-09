namespace WalletService.Application.Models
{
    /// <summary>
    /// Wallet-id-resolved version of <see cref="TradeSettlementRequest"/>, passed to
    /// <c>ITradeSettlementRepository</c> once both users' wallets have been resolved.
    /// </summary>
    public sealed class TradeSettlementCommand
    {
        public required Guid TradeId { get; set; }
        public required long BuyerWalletId { get; set; }
        public required long SellerWalletId { get; set; }
        public required long BaseAssetId { get; set; }
        public required long QuoteAssetId { get; set; }
        public required decimal Quantity { get; set; }
        public required decimal QuoteAmount { get; set; }
        public required string ActorName { get; set; }
    }
}
