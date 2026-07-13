namespace WalletService.Application.Models
{
    public class SendRequest
    {
        public Guid UserId { get; set; }

        /// <summary>Asset symbol to withdraw. Defaults to ETH when omitted.</summary>
        public string AssetSymbol { get; set; } = "ETH";

        public required string RecipientAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
