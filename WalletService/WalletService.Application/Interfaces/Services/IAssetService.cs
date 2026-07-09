using WalletService.Application.Dto;

namespace WalletService.Application.Interfaces.Services
{
    public interface IAssetService
    {
        Task<AssetDto> GetOrCreateByNameAsync(string assetName, CancellationToken cancellationToken = default);
    }
}
