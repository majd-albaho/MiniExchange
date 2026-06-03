using WalletService.Domain.Enums;

namespace WalletService.Application.Dto
{
    public class UserWalletAddressDto
    {
        public required long Id { get; set; }
        public required long UserWalletId { get; set; }

        public required string Address { get; set; }

        public required string PrivateKey { get; set; }
        public CryptoNetworkType CryptoNetworkType { get; set; }
    }
}
