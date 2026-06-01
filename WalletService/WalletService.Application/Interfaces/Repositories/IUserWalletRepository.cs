using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletRepository
    {
        Task<UserWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default);
        Task<bool> TryLockFundsAsync(long walletId, decimal amount, decimal totalBalance, string modifiedBy, CancellationToken cancellationToken = default);
        Task<bool> TryUnlockFundsAsync(long walletId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
