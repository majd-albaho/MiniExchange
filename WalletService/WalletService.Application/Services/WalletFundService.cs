using Microsoft.Extensions.Logging;
using WalletService.Application.Dto;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Services
{
    public class WalletFundService : IWalletFundService
    {
        private readonly IUserWalletAssetsRepository _userWalletAssetsRepository;
        private readonly IUserWalletService _userWalletService;
        private readonly IAssetService _assetService;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly ILogger<WalletFundService> _logger;

        public WalletFundService(
            IUserWalletAssetsRepository userWalletAssetsRepository,
            IUserWalletService userWalletService,
            IAssetService assetService,
            IWalletTransactionRepository walletTransactionRepository,
            ILogger<WalletFundService> logger)
        {
            _userWalletAssetsRepository = userWalletAssetsRepository;
            _userWalletService = userWalletService;
            _assetService = assetService;
            _walletTransactionRepository = walletTransactionRepository;
            _logger = logger;
        }

        public async Task LockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);

            var userWallet = await _userWalletService.GetUserWallet(userId, cancellationToken);
            await _userWalletAssetsRepository.LockFundsAsync(userWallet.Id, assetId, amount, userId.ToString(), cancellationToken);

            _logger.LogInformation("Locked {Amount} of asset {AssetId} for user {UserId} in wallet {WalletId}", amount, assetId, userId, userWallet.Id);
        }

        public async Task UnlockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);

            var wallet = await _userWalletService.GetUserWallet(userId, cancellationToken);
            await _userWalletAssetsRepository.UnlockFundsAsync(wallet.Id, assetId, amount, userId.ToString(), cancellationToken);

            _logger.LogInformation("Unlocked {Amount} of asset {AssetId} for user {UserId} in wallet {WalletId}", amount, assetId, userId, wallet.Id);
        }

        public async Task CreditFund(Guid userId, string assetName, decimal amount, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);

            var asset = await _assetService.GetOrCreateByNameAsync(assetName, cancellationToken);
            await CreditLedgerAsync(userId, asset, amount, WalletTransactionType.Credit, referenceId: null, externalReference: null, cancellationToken);

            _logger.LogInformation(
                "[DEV/TEST CREDIT] Credited {Amount} of asset {AssetName} ({AssetId}) to user {UserId}",
                amount, asset.AssetName, asset.Id, userId);
        }

        public async Task AddDemoTokenAsync(Guid userId, string assetName, decimal amount, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);

            if (SupportedAssets.ReservedRealSymbols.Contains(assetName.Trim()))
                throw new InvalidOperationException($"{assetName.Trim().ToUpperInvariant()} is a real on-chain asset and cannot be added as a demo token.");

            var existing = await _assetService.GetByNameAsync(assetName, cancellationToken);
            if (existing is not null && !existing.IsDemo)
                throw new InvalidOperationException($"Asset {existing.AssetName} already exists as a real asset and cannot be credited as a demo token.");

            var asset = await _assetService.GetOrCreateByNameAsync(assetName, isDemo: true, CryptoNetworkType.None, cancellationToken);
            await CreditLedgerAsync(userId, asset, amount, WalletTransactionType.Credit, referenceId: null, externalReference: null, cancellationToken);

            _logger.LogInformation(
                "[DEMO TOKEN] Added {Amount} of demo token {AssetName} ({AssetId}) to user {UserId}",
                amount, asset.AssetName, asset.Id, userId);
        }

        public async Task RecordWithdrawalAsync(Guid userId, long assetId, decimal amount, string transactionHash, CancellationToken cancellationToken = default)
        {
            var wallet = await _userWalletService.GetUserWallet(userId, cancellationToken);
            var updated = await _userWalletAssetsRepository.DebitAsync(wallet.Id, assetId, amount, userId.ToString(), cancellationToken);

            await _walletTransactionRepository.RecordAsync(new WalletTransaction
            {
                Id = default,
                UserWalletId = wallet.Id,
                AssetId = assetId,
                Type = WalletTransactionType.Withdrawal,
                Amount = amount,
                BalanceAfter = updated.Amount,
                ReferenceId = null,
                ExternalReference = transactionHash,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            }, cancellationToken);

            _logger.LogInformation("Recorded withdrawal of {Amount} (asset {AssetId}) for user {UserId}, tx {TxHash}", amount, assetId, userId, transactionHash);
        }

        public async Task RecordDepositAsync(Guid userId, string assetName, decimal amount, string transactionHash, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);

            var asset = await _assetService.GetOrCreateByNameAsync(assetName, isDemo: false, SupportedAssets.NetworkFor(assetName), cancellationToken);
            await CreditLedgerAsync(userId, asset, amount, WalletTransactionType.Deposit, referenceId: null, transactionHash, cancellationToken);

            _logger.LogInformation(
                "[DEPOSIT] Credited {Amount} of {AssetName} to user {UserId} from on-chain tx {TxHash}",
                amount, asset.AssetName, userId, transactionHash);
        }

        private async Task CreditLedgerAsync(Guid userId, AssetDto asset, decimal amount, WalletTransactionType type, Guid? referenceId, string? externalReference, CancellationToken cancellationToken)
        {
            var wallet = await _userWalletService.GetUserWallet(userId, cancellationToken);
            var updated = await _userWalletAssetsRepository.CreditAsync(wallet.Id, asset.Id, amount, userId.ToString(), cancellationToken);

            await _walletTransactionRepository.RecordAsync(new WalletTransaction
            {
                Id = default,
                UserWalletId = wallet.Id,
                AssetId = asset.Id,
                Type = type,
                Amount = amount,
                BalanceAfter = updated.Amount,
                ReferenceId = referenceId,
                ExternalReference = externalReference,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        private static void EnsurePositiveAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero");
        }
    }
}
