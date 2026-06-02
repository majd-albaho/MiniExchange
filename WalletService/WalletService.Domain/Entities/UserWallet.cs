using SharedLibrary.Entities;

namespace WalletService.Domain.Entities
{
    public class UserWallet : EntityBase<long>
    {
        public required Guid UserId { get; set; }
        public required long WalletId { get; set; }

        public decimal LockedBalance { get; set; }
    }
}
