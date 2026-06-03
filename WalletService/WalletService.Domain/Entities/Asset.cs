using SharedLibrary.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Domain.Entities
{
    public class Asset : EntityBase<long>
    {
        public required string AssetName { get; set; }
        public CryptoNetworkType CryptoNetworkType { get; set; }
    }
}
