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

        public Task<UserWallet?> GetByIdAsync(long walletId, CancellationToken cancellationToken = default)
        {
            return _context.UserWallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == walletId, cancellationToken);
        }

        public async Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default)
        {
            await _context.UserWallets.AddAsync(userWallet, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return userWallet;
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
