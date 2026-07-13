using Microsoft.Extensions.Logging;
using WalletService.Application.Dto;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Services
{
    public class WalletOverviewService : IWalletOverviewService
    {
        private readonly IUserWalletService _userWalletService;
        private readonly IUserWalletAssetsRepository _userWalletAssetsRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly IAssetService _assetService;
        private readonly IWalletBlockchainClient _walletBlockchainClient;
        private readonly ILogger<WalletOverviewService> _logger;

        public WalletOverviewService(
            IUserWalletService userWalletService,
            IUserWalletAssetsRepository userWalletAssetsRepository,
            IAssetRepository assetRepository,
            IAssetService assetService,
            IWalletBlockchainClient walletBlockchainClient,
            ILogger<WalletOverviewService> logger)
        {
            _userWalletService = userWalletService;
            _userWalletAssetsRepository = userWalletAssetsRepository;
            _assetRepository = assetRepository;
            _assetService = assetService;
            _walletBlockchainClient = walletBlockchainClient;
            _logger = logger;
        }

        public async Task<WalletOverviewDto> GetOverviewAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var wallet = await _userWalletService.GetUserWallet(userId, cancellationToken);
            var ethAddress = await _userWalletService.GetUserWalletAddress(userId, CryptoNetworkType.Ethereum, cancellationToken);

            var balances = await _userWalletAssetsRepository.ListByWalletAsync(wallet.Id, cancellationToken);
            var assetsById = (await _assetRepository.ListByIdsAsync(balances.Select(b => b.AssetId).Distinct().ToArray(), cancellationToken))
                .ToDictionary(a => a.Id);

            var overview = new WalletOverviewDto();

            var ethLedger = balances.FirstOrDefault(b =>
                assetsById.TryGetValue(b.AssetId, out var a) && a.AssetName == SupportedAssets.Ethereum);

            overview.Assets.Add(new WalletBalanceDto
            {
                Id = ethLedger?.AssetId.ToString() ?? SupportedAssets.Ethereum,
                Symbol = SupportedAssets.Ethereum,
                Name = "Ethereum",
                Network = "Ethereum (Sepolia)",
                Balance = await GetLiveEthBalanceAsync(ethAddress.PublicAddress, cancellationToken),
                LockedBalance = ethLedger?.LockedAmount ?? 0m,
                DepositAddress = ethAddress.PublicAddress,
                IsDemo = false
            });

            foreach (var balance in balances)
            {
                if (!assetsById.TryGetValue(balance.AssetId, out var asset) || asset.AssetName == SupportedAssets.Ethereum)
                    continue;

                overview.Assets.Add(new WalletBalanceDto
                {
                    Id = asset.Id.ToString(),
                    Symbol = asset.AssetName,
                    Name = asset.AssetName,
                    Network = NetworkLabel(asset),
                    Balance = balance.Amount,
                    LockedBalance = balance.LockedAmount,
                    DepositAddress = string.Empty,
                    IsDemo = asset.IsDemo
                });
            }

            return overview;
        }

        public async Task<ReceiveInfoDto> GetReceiveInfoAsync(Guid userId, string symbol, string network, CancellationToken cancellationToken = default)
        {
            var normalized = string.IsNullOrWhiteSpace(symbol) ? SupportedAssets.Ethereum : symbol.Trim().ToUpperInvariant();

            if (normalized == SupportedAssets.Ethereum)
            {
                var address = await _userWalletService.GetUserWalletAddress(userId, CryptoNetworkType.Ethereum, cancellationToken);
                return new ReceiveInfoDto
                {
                    Symbol = normalized,
                    Network = "Ethereum (Sepolia)",
                    Address = address.PublicAddress,
                    MinDeposit = 0.001m,
                    Confirmations = 12,
                    IsDemo = false
                };
            }

            var asset = await _assetService.GetByNameAsync(normalized, cancellationToken);
            return new ReceiveInfoDto
            {
                Symbol = normalized,
                Network = string.IsNullOrWhiteSpace(network) ? "Demo" : network,
                Address = string.Empty,
                IsDemo = asset?.IsDemo ?? true
            };
        }

        private async Task<decimal> GetLiveEthBalanceAsync(string address, CancellationToken cancellationToken)
        {
            try
            {
                return await _walletBlockchainClient.GetEthereumBalanceAsync(address, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read live ETH balance for {Address}. Returning 0.", address);
                return 0m;
            }
        }

        private static string NetworkLabel(Asset asset)
        {
            if (asset.IsDemo)
                return "Demo";

            return asset.CryptoNetworkType switch
            {
                CryptoNetworkType.Ethereum => "Ethereum",
                CryptoNetworkType.Bitcoin => "Bitcoin",
                CryptoNetworkType.BinanceSmartChain => "BSC",
                CryptoNetworkType.Polygon => "Polygon",
                CryptoNetworkType.Solana => "Solana",
                _ => "Ledger"
            };
        }
    }
}
