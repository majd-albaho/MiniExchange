using Microsoft.EntityFrameworkCore;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Persistence;

namespace WalletService.Infrastructure.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly WalletDbContext _context;

        public AssetRepository(WalletDbContext context)
        {
            _context = context;
        }

        public Task<Asset?> GetByNameAsync(string assetName, CancellationToken cancellationToken = default)
        {
            return _context.Assets.FirstOrDefaultAsync(a => a.AssetName == assetName, cancellationToken);
        }

        public async Task<IReadOnlyList<Asset>> ListByIdsAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken = default)
        {
            return await _context.Assets.AsNoTracking()
                .Where(a => ids.Contains(a.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default)
        {
            await _context.Assets.AddAsync(asset, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return asset;
        }
    }
}
