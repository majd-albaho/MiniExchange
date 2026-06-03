using WalletService.Application.Dto;
using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IUserWalletRepository
    {
        Task<UserWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
