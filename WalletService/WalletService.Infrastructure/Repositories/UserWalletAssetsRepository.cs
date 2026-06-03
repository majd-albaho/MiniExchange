using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
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

        public async Task LockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await _context.UserWalletAssets
                .FirstOrDefaultAsync(w => w.UserWalletId == userWalletId && w.AssetId == assetId, cancellationToken);
            if (userWalletAsset == null)
                throw new InvalidOperationException("User wallet asset not found");

            if (userWalletAsset.AvailableAmount < amount)
                throw new InvalidOperationException("Insufficient available balance to lock funds");

            userWalletAsset.LockedAmount += amount;
            userWalletAsset.ModifiedBy = modifiedBy;
            userWalletAsset.ModifiedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UnlockFundsAsync(long userWalletId, long assetId, decimal amount, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var userWalletAsset = await _context.UserWalletAssets
                .FirstOrDefaultAsync(w => w.UserWalletId == userWalletId && w.AssetId == assetId, cancellationToken);
            if (userWalletAsset == null)
                throw new InvalidOperationException("User wallet asset not found");

            if (userWalletAsset.LockedAmount < amount)
                throw new InvalidOperationException("Insufficient locked balance to unlock funds");

            userWalletAsset.LockedAmount -= amount;
            userWalletAsset.ModifiedBy = modifiedBy;
            userWalletAsset.ModifiedDate = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
