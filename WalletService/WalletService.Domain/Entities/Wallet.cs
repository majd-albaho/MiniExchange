using SharedLibrary.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Domain.Entities
{
    public class Wallet : EntityBase<long>
    {
        public string WalletName { get; set; } = string.Empty;
        public required string Address { get; set; }
        public required string PrivateKey { get; set; }
        public CryptoNetworkType CryptoNetworkType { get; set; }
    }
}
