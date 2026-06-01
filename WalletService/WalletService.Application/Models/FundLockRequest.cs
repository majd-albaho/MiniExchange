namespace WalletService.Application.Models
{
    public sealed class FundLockRequest
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
