using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletAssetsRepository
    {
        Task<UserWalletAsset> GetOrCreateAsync(long userWalletId, long assetId, string createdBy, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserWalletAsset>> ListByWalletAsync(long userWalletId, CancellationToken cancellationToken = default);
        Task LockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
        Task UnlockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
        Task<UserWalletAsset> CreditAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);

        /// <summary>Reduces the running balance (does not touch locked funds). Clamps at zero.</summary>
        Task<UserWalletAsset> DebitAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
    }
}
