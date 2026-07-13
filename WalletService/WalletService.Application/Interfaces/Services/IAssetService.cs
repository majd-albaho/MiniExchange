using WalletService.Application.Dto;
using WalletService.Domain.Enums;

namespace WalletService.Application.Interfaces.Services
{
    public interface IAssetService
    {
        Task<AssetDto> GetOrCreateByNameAsync(string assetName, CancellationToken cancellationToken = default);

        Task<AssetDto> GetOrCreateByNameAsync(string assetName, bool isDemo, CryptoNetworkType networkType, CancellationToken cancellationToken = default);

        Task<AssetDto?> GetByNameAsync(string assetName, CancellationToken cancellationToken = default);
    }
}
