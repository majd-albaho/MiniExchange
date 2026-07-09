using WalletService.Domain.Entities;

namespace WalletService.Application.Interfaces.Repositories
{
    public interface IAssetRepository
    {
        Task<Asset?> GetByNameAsync(string assetName, CancellationToken cancellationToken = default);
        Task<Asset> CreateAsync(Asset asset, CancellationToken cancellationToken = default);
    }
}
