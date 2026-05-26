namespace WalletService.Application.Models
{
    public class SendRequest
    {
        public Guid UserId { get; set; }
        public required string RecipientAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
