using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletAddressRepository
    {
        Task<UserWalletAddress?> GetByUserWalletId(long userWalletId, CryptoNetworkType cryptoNetworkType, CancellationToken cancellationToken = default);
        Task<UserWalletAddress> AddAsync(UserWalletAddress userWalletAddress, CancellationToken cancellationToken = default);
    }
}
