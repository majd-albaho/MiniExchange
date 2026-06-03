using Microsoft.EntityFrameworkCore;
using SharedLibrary.Entities;

namespace WalletService.Domain.Entities
{
    [Index(nameof(UserWalletId), nameof(AssetId), IsUnique = true)]
    [Index(nameof(AssetId), IsUnique = false)]
    public class UserWalletAsset : EntityBase<long>
    {
        public required long UserWalletId { get; set; }
        public required long AssetId { get; set; }

        public decimal Amount { get; set; }
        public decimal LockedAmount { get; set; }

        public decimal AvailableAmount => Amount - LockedAmount;
    }
}
