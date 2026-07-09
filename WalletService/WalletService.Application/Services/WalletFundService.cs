using Microsoft.Extensions.Logging;
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
            var wallet = await _userWalletService.GetUserWallet(userId, cancellationToken);
            var updated = await _userWalletAssetsRepository.CreditAsync(wallet.Id, asset.Id, amount, userId.ToString(), cancellationToken);

            await _walletTransactionRepository.RecordAsync(new WalletTransaction
            {
                Id = default,
                UserWalletId = wallet.Id,
                AssetId = asset.Id,
                Type = WalletTransactionType.Credit,
                Amount = amount,
                BalanceAfter = updated.Amount,
                ReferenceId = null,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            }, cancellationToken);

            _logger.LogInformation(
                "[DEV/TEST CREDIT] Credited {Amount} of asset {AssetName} ({AssetId}) to user {UserId} in wallet {WalletId}",
                amount, asset.AssetName, asset.Id, userId, wallet.Id);
        }

        private static void EnsurePositiveAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero");
        }
    }
}
