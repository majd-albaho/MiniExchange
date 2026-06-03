using System.Numerics;
using WalletService.Domain.Enums;

namespace WalletService.Application.Dto
{
    public class UserWalletTransactionDto
    {
        public required long Id { get; set; }
        public int UserWalletAddressId { get; set; }
        public long AssetId { get; set; }

        public DateTimeOffset TransactionDateTime { get; set; }
        public TransactionType TransactionType { get; set; }

        public required string TransactionHash { get; set; }
        public required string From { get; set; }
        public required string To { get; set; }

        public bool Failed { get; set; }
        public BigInteger BlockNumber { get; set; }
        public BigInteger GasUsed { get; set; }
        public BigInteger EffectiveGasPrice { get; set; }

        public decimal Amount { get; set; }
    }
}
