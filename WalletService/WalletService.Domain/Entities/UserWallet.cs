using SharedLibrary.Entities;

namespace WalletService.Domain.Entities
{
    public class UserWallet : EntityBase<long>
    {
        public required string UserId { get; set; }
        public required string Address { get; set; }
        public required string PrivateKey { get; set; }
    }
}
