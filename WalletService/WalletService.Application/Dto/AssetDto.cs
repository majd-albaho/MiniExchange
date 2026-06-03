using WalletService.Domain.Enums;

namespace WalletService.Application.Dto
{
    public class AssetDto
    {
        public required long Id { get; set; }
        public required string AssetName { get; set; }
        public CryptoNetworkType CryptoNetworkType { get; set; }
    }
}
