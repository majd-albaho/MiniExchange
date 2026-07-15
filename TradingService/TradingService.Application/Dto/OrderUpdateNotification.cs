namespace TradingService.Application.Dto
{
    /// <summary>
    /// Pushed to a user when one of their orders is (partially) filled, so the UI can update the
    /// order and refresh balances without polling. Enums are sent as strings for the browser.
    /// </summary>
    public sealed class OrderUpdateNotification
    {
        public Guid OrderId { get; set; }
        public required string PairSymbol { get; set; }
        public required string Side { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
        public decimal Quantity { get; set; }
        public decimal FilledQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal LastFillQuantity { get; set; }
        public decimal LastFillPrice { get; set; }
    }
}
