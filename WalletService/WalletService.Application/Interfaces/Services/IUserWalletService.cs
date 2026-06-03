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
    }
}
