using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace BlockchainScanner.Entites
{
    [Index(nameof(TransactionDateTime), IsUnique = false)]
    [Index(nameof(TransactionType), IsUnique = false)]
    public class BlockchainTransaction
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public required string Asset { get; set; }

        public DateTimeOffset TransactionDateTime { get; set; }
        public TransactionType TransactionType { get; set; }

        [MaxLength(255)]
        public required string TransactionHash { get; set; }

        [MaxLength(255)]
        public required string From { get; set; }

        [MaxLength(255)]
        public required string To { get; set; }

        public bool Confirmed { get; set; }
        public BigInteger BlockNumber { get; set; }
        public BigInteger GasUsed { get; set; }
        public BigInteger EffectiveGasPrice { get; set; }

        public decimal Amount { get; set; }
    }
}
