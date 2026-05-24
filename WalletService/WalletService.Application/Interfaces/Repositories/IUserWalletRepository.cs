using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletRepository
    {
        Task<UserWallet?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default);
    }
}
