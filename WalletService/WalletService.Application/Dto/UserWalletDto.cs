namespace WalletService.Application.Dto
{
    public class UserWalletDto
    {
        public required long Id { get; set; }
        public required Guid UserId { get; set; }

        public string WalletName { get; set; } = string.Empty;
    }
}
