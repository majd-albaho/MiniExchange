using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Enums;

namespace WalletService.Application.Services
{
    public class WalletTransactionService : IWalletTransactionService
    {
        private readonly IUserWalletService _userWalletService;
        private readonly IWalletBlockchainClient _walletBlockchainClient;
        private readonly IAssetService _assetService;
        private readonly IWalletFundService _walletFundService;
        private readonly ILogger<WalletTransactionService> _logger;

        public WalletTransactionService(
            IUserWalletService userWalletService,
            IWalletBlockchainClient walletBlockchainClient,
            IAssetService assetService,
            IWalletFundService walletFundService,
            ILogger<WalletTransactionService> logger)
        {
            _userWalletService = userWalletService;
            _walletBlockchainClient = walletBlockchainClient;
            _assetService = assetService;
            _walletFundService = walletFundService;
            _logger = logger;
        }

        public async Task<string> Send(Guid userId, string assetSymbol, string recipientAddress, decimal amount, CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero");

            var symbol = string.IsNullOrWhiteSpace(assetSymbol)
                ? SupportedAssets.Ethereum
                : assetSymbol.Trim().ToUpperInvariant();

            var asset = await ResolveWithdrawableAssetAsync(symbol, cancellationToken);

            _logger.LogInformation("Initiating on-chain transfer of {Amount} {Symbol} from user {UserId} to {RecipientAddress}", amount, symbol, userId, recipientAddress);

            var wallet = await _userWalletService.GetUserWalletAddress(userId, CryptoNetworkType.Ethereum, cancellationToken);
            var transactionHash = await _walletBlockchainClient.SendEthereumAsync(wallet.PrivateKey, recipientAddress, amount, Chain.Sepolia, cancellationToken);

            // Chain is authoritative for real balances; mirror the withdrawal into the ledger for history/overview.
            await _walletFundService.RecordWithdrawalAsync(userId, asset.Id, amount, transactionHash, cancellationToken);

            _logger.LogInformation("{Symbol} transfer succeeded. TxHash: {TransactionHash} From user: {UserId} To: {RecipientAddress} Amount: {Amount}", symbol, transactionHash, userId, recipientAddress, amount);
            return transactionHash;
        }

        private async Task<Dto.AssetDto> ResolveWithdrawableAssetAsync(string symbol, CancellationToken cancellationToken)
        {
            var asset = symbol == SupportedAssets.Ethereum
                ? await _assetService.GetOrCreateByNameAsync(symbol, isDemo: false, CryptoNetworkType.Ethereum, cancellationToken)
                : await _assetService.GetByNameAsync(symbol, cancellationToken)
                    ?? throw new InvalidOperationException($"Unknown asset {symbol}.");

            if (!SupportedAssets.IsWithdrawable(asset.IsDemo, asset.CryptoNetworkType))
                throw new InvalidOperationException(
                    $"{symbol} is a demo/test token and cannot be sent on-chain. Only real on-chain assets (ETH) can be withdrawn.");

            return asset;
        }

        public async Task<string> GetTransactionDetails(string transactionId, CancellationToken cancellationToken = default)
        {
            var result = await _walletBlockchainClient.GetTransactionDetailsAsync(transactionId, cancellationToken);
            _logger.LogInformation("Transaction details retrieved for transaction {TransactionId}: {TransactionDetails}", transactionId, result);
            return result;
        }
    }
}
