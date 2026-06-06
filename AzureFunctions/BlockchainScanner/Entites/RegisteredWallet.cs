using System.ComponentModel.DataAnnotations;

namespace BlockchainScanner.Entites
{
    public class RegisteredWallet
    {
        public int Id { get; set; }
        public required Guid UserId { get; set; }

        [MaxLength(255)]
        public required string PublicAddress { get; set; }
    }
}
