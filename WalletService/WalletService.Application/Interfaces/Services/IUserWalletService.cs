using WalletService.Application.Dto;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Interfaces.Services
{
    public interface IUserWalletService
    {
        Task<UserWalletDto> GetUserWallet(Guid userId, CancellationToken cancellationToken = default);
        Task<UserWalletAddress> GetUserWalletAddress(Guid userId, CryptoNetworkType networkType, CancellationToken cancellationToken = default);
        Task<decimal> CheckEthereumBalance(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Maps an on-chain public address back to the owning user, or null if it isn't one of ours.</summary>
        Task<Guid?> ResolveUserIdByAddressAsync(string publicAddress, CancellationToken cancellationToken = default);
    }
}
