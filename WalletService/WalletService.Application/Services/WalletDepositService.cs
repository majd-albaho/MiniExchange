using Microsoft.Extensions.Logging;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;

namespace WalletService.Application.Services
{
    public class WalletDepositService : IWalletDepositService
    {
        private readonly IUserWalletService _userWalletService;
        private readonly IWalletFundService _walletFundService;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly ILogger<WalletDepositService> _logger;

        public WalletDepositService(
            IUserWalletService userWalletService,
            IWalletFundService walletFundService,
            IWalletTransactionRepository walletTransactionRepository,
            ILogger<WalletDepositService> logger)
        {
            _userWalletService = userWalletService;
            _walletFundService = walletFundService;
            _walletTransactionRepository = walletTransactionRepository;
            _logger = logger;
        }

        public async Task<bool> ProcessDepositAsync(string toAddress, string assetSymbol, decimal amount, string transactionHash, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toAddress) || string.IsNullOrWhiteSpace(transactionHash) || amount <= 0)
            {
                _logger.LogWarning("Ignoring malformed deposit activity. To: {To}, Amount: {Amount}, Tx: {TxHash}", toAddress, amount, transactionHash);
                return false;
            }

            if (await _walletTransactionRepository.ExistsByExternalReferenceAsync(transactionHash, cancellationToken))
            {
                _logger.LogInformation("Deposit tx {TxHash} already processed. Skipping.", transactionHash);
                return false;
            }

            var userId = await _userWalletService.ResolveUserIdByAddressAsync(toAddress, cancellationToken);
            if (userId is null)
            {
                _logger.LogInformation("Deposit to address {Address} does not belong to any wallet. Ignoring.", toAddress);
                return false;
            }

            var symbol = string.IsNullOrWhiteSpace(assetSymbol) ? "ETH" : assetSymbol;
            await _walletFundService.RecordDepositAsync(userId.Value, symbol, amount, transactionHash, cancellationToken);
            return true;
        }
    }
}
