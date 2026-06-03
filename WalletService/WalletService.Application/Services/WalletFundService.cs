using Microsoft.Extensions.Logging;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;

namespace WalletService.Application.Services
{
    public class WalletFundService : IWalletFundService
    {
        private readonly IUserWalletAssetsRepository _userWalletAssetsRepository;
        private readonly IUserWalletService _userWalletService;
        private readonly ILogger<WalletFundService> _logger;

        public WalletFundService(
            IUserWalletAssetsRepository userWalletAssetsRepository,
            IUserWalletService userWalletService,
            ILogger<WalletFundService> logger)
        {
            _userWalletAssetsRepository = userWalletAssetsRepository;
            _userWalletService = userWalletService;
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

        private static void EnsurePositiveAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero");
        }
    }
}
