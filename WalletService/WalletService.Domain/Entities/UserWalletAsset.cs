using SharedLibrary.Entities;

namespace WalletService.Domain.Entities
{
    public class UserWalletAsset : EntityBase<long>
    {
        public long UserWalletId { get; set; }
        public long AssetId { get; set; }
        public decimal Amount { get; set; }
        public decimal LockedAmount { get; set; }
        public decimal AvailableAmount => Amount - LockedAmount;
    }
}
