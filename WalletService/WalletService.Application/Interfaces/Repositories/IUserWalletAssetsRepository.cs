using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletAssetsRepository
    {
        Task<UserWalletAsset> GetOrCreateAsync(long userWalletId, long assetId, string createdBy, CancellationToken cancellationToken = default);
        Task LockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
        Task UnlockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
        Task<UserWalletAsset> CreditAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
    }
}
