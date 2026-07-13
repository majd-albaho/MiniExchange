using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    public class UserWalletAssetsRepository : IUserWalletAssetsRepository
    {
        private readonly WalletDbContext _context;

        public UserWalletAssetsRepository(WalletDbContext context)
        {
            _context = context;
        }

        public async Task<UserWalletAsset> GetOrCreateAsync(long userWalletId, long assetId, string createdBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await _context.UserWalletAssets
                .FirstOrDefaultAsync(w => w.UserWalletId == userWalletId && w.AssetId == assetId, cancellationToken);

            if (userWalletAsset is not null)
            {
                return userWalletAsset;
            }

            userWalletAsset = new UserWalletAsset
            {
                Id = default,
                UserWalletId = userWalletId,
                AssetId = assetId,
                Amount = 0m,
                LockedAmount = 0m,
                CreatedBy = createdBy,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await _context.UserWalletAssets.AddAsync(userWalletAsset, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return userWalletAsset;
        }

        public async Task<IReadOnlyList<UserWalletAsset>> ListByWalletAsync(long userWalletId, CancellationToken cancellationToken = default)
        {
            return await _context.UserWalletAssets.AsNoTracking()
                .Where(w => w.UserWalletId == userWalletId)
                .ToListAsync(cancellationToken);
        }

        public async Task LockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await GetOrCreateAsync(userWalletId, assetId, modifiedBy, cancellationToken);

            if (userWalletAsset.AvailableAmount < amount)
                throw new InvalidOperationException("Insufficient available balance to lock funds");

            userWalletAsset.LockedAmount += amount;
            userWalletAsset.ModifiedBy = modifiedBy;
            userWalletAsset.ModifiedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UnlockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await GetOrCreateAsync(userWalletId, assetId, modifiedBy, cancellationToken);

            if (userWalletAsset.LockedAmount < amount)
                throw new InvalidOperationException("Insufficient locked balance to unlock funds");

            userWalletAsset.LockedAmount -= amount;
            userWalletAsset.ModifiedBy = modifiedBy;
            userWalletAsset.ModifiedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserWalletAsset> CreditAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await GetOrCreateAsync(userWalletId, assetId, modifiedBy, cancellationToken);

            userWalletAsset.Amount += amount;
            userWalletAsset.ModifiedBy = modifiedBy;
            userWalletAsset.ModifiedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return userWalletAsset;
        }

        public async Task<UserWalletAsset> DebitAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await GetOrCreateAsync(userWalletId, assetId, modifiedBy, cancellationToken);

            userWalletAsset.Amount = userWalletAsset.Amount > amount ? userWalletAsset.Amount - amount : 0m;
            userWalletAsset.ModifiedBy = modifiedBy;
            userWalletAsset.ModifiedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return userWalletAsset;
        }
    }
}
