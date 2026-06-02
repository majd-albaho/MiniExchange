using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    internal class UserWalletRepository : IUserWalletRepository
    {
        private readonly WalletDbContext _context;

        public UserWalletRepository(WalletDbContext context)
        {
            _context = context;
        }

        public Task<UserWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _context.UserWallets.AsNoTracking().FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }

        public async Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default)
        {
            await _context.UserWallets.AddAsync(userWallet, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return userWallet;
        }

        public async Task<bool> TryLockFundsAsync(long walletId, decimal amount, decimal totalBalance, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var availableBalanceFloor = totalBalance - amount;
            var modifiedDate = DateTimeOffset.UtcNow;

            var updatedRows = await _context.UserWallets
                .Where(w => w.Id == walletId && w.LockedBalance <= availableBalanceFloor)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(w => w.LockedBalance, w => w.LockedBalance + amount)
                    .SetProperty(w => w.ModifiedBy, modifiedBy)
                    .SetProperty(w => w.ModifiedDate, modifiedDate), cancellationToken);

            return updatedRows == 1;
        }

        public async Task<bool> TryUnlockFundsAsync(long walletId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var modifiedDate = DateTimeOffset.UtcNow;

            var updatedRows = await _context.UserWallets
                .Where(w => w.Id == walletId && w.LockedBalance >= amount)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(w => w.LockedBalance, w => w.LockedBalance - amount)
                    .SetProperty(w => w.ModifiedBy, modifiedBy)
                    .SetProperty(w => w.ModifiedDate, modifiedDate), cancellationToken);

            return updatedRows == 1;
        }

        public async Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var userWallet = await GetByUserIdAsync(userId, cancellationToken);
            if (userWallet == null)
            {
                return false;
            }

            _context.UserWallets.Remove(userWallet);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
