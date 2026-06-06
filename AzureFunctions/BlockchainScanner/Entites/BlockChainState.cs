using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace BlockchainScanner.Entites
{
    [PrimaryKey("Network")]
    public class BlockChainState
    {
        [MaxLength(20)]
        public required string Network { get; set; }
        public BigInteger LastProcessedBlock { get; set; }
    }
}
