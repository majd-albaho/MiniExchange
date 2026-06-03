using Microsoft.EntityFrameworkCore;
using SharedLibrary.Entities;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using WalletService.Domain.Enums;

namespace WalletService.Domain.Entities
{
    [Index(nameof(UserWalletAddressId), nameof(AssetId), IsUnique = false)]
    [Index(nameof(TransactionDateTime), IsUnique = false)]
    [Index(nameof(TransactionType), IsUnique = false)]
    public class UserWalletTransaction : EntityBase<long>
    {
        public int UserWalletAddressId { get; set; }
        public long AssetId { get; set; }

        public DateTimeOffset TransactionDateTime { get; set; }
        public TransactionType TransactionType { get; set; }

        [MaxLength(255)]
        public required string TransactionHash { get; set; }

        [MaxLength(255)]
        public required string From { get; set; }

        [MaxLength(255)]
        public required string To { get; set; }

        public bool Failed { get; set; }
        public BigInteger BlockNumber { get; set; }
        public BigInteger GasUsed { get; set; }
        public BigInteger EffectiveGasPrice { get; set; }

        public decimal Amount { get; set; }
    }
}
