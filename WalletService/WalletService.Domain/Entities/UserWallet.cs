using SharedLibrary.Entities;
using System.ComponentModel.DataAnnotations;

namespace WalletService.Domain.Entities
{
    public class UserWallet : EntityBase<long>
    {
        public required Guid UserId { get; set; }

        [MaxLength(255)]
        public string WalletName { get; set; } = string.Empty;
    }
}
