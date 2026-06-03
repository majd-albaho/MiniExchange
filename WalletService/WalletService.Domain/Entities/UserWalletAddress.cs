using Microsoft.EntityFrameworkCore;
using SharedLibrary.Entities;
using System.ComponentModel.DataAnnotations;
using WalletService.Domain.Enums;

namespace WalletService.Domain.Entities
{
    [Index(nameof(UserWalletId), nameof(CryptoNetworkType), IsUnique = true)]
    public class UserWalletAddress : EntityBase<long>
    {
        public required long UserWalletId { get; set; }

        [MaxLength(255)]
        public required string PublicAddress { get; set; }

        [MaxLength(255)]
        public required string PrivateKey { get; set; }


        public CryptoNetworkType CryptoNetworkType { get; set; }
    }
}
