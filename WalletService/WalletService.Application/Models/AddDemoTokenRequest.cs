namespace WalletService.Application.Models
{
    public sealed class AddDemoTokenRequest
    {
        public Guid UserId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
