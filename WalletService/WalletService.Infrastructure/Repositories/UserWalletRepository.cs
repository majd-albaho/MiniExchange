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

        public async Task<UserWallet?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserWallets
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
        }

        public async Task<UserWallet> CreateAsync(UserWallet userWallet, CancellationToken cancellationToken = default)
        {
            await _context.UserWallets.AddAsync(userWallet, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return userWallet;
        }

        public async Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default)
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
