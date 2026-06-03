namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletAssetsRepository
    {
        Task LockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
        Task UnlockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
    }
}
