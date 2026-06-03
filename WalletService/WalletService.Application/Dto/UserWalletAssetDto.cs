namespace WalletService.Application.Dto
{
    public class UserWalletAssetDto
    {
        public required long Id { get; set; }
        public required long UserWalletId { get; set; }

        public required long AssetId { get; set; }

        public decimal Amount { get; set; }
        public decimal LockedAmount { get; set; }

        public decimal AvailableAmount => Amount - LockedAmount;
    }
}
