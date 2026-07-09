using Microsoft.Extensions.Logging;
using WalletService.Application.Dto;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Services
{
    public class AssetService : IAssetService
    {
        private const string SystemActor = "WalletService";

        private readonly IAssetRepository _assetRepository;
        private readonly ILogger<AssetService> _logger;

        public AssetService(IAssetRepository assetRepository, ILogger<AssetService> logger)
        {
            _assetRepository = assetRepository;
            _logger = logger;
        }

        public async Task<AssetDto> GetOrCreateByNameAsync(string assetName, CancellationToken cancellationToken = default)
        {
            var normalizedName = NormalizeAssetName(assetName);

            var asset = await _assetRepository.GetByNameAsync(normalizedName, cancellationToken);
            if (asset is null)
            {
                _logger.LogInformation("No asset found named {AssetName}. Creating new ledger asset.", normalizedName);

                asset = await _assetRepository.CreateAsync(new Asset
                {
                    Id = default,
                    AssetName = normalizedName,
                    CryptoNetworkType = CryptoNetworkType.None,
                    CreatedBy = SystemActor,
                    CreatedDate = DateTimeOffset.UtcNow
                }, cancellationToken);
            }

            return Map(asset);
        }

        private static string NormalizeAssetName(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                throw new ArgumentException("Asset name is required.", nameof(assetName));

            return assetName.Trim().ToUpperInvariant();
        }

        private static AssetDto Map(Asset asset)
        {
            return new AssetDto
            {
                Id = asset.Id,
                AssetName = asset.AssetName,
                CryptoNetworkType = asset.CryptoNetworkType
            };
        }
    }
}
