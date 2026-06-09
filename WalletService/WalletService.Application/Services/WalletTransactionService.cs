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
        private readonly ILogger<WalletTransactionService> _logger;

        public WalletTransactionService(IUserWalletService userWalletService, IWalletBlockchainClient walletBlockchainClient, ILogger<WalletTransactionService> logger)
        {
            _userWalletService = userWalletService;
            _walletBlockchainClient = walletBlockchainClient;
            _logger = logger;
        }

        public async Task<string> SendEthereum(Guid userId, string recipientAddress, decimal amount, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initiating transfer of {Amount} ETH from user {UserId} to {RecipientAddress}", amount, userId, recipientAddress);

            var wallet = await _userWalletService.GetUserWalletAddress(userId, CryptoNetworkType.Ethereum, cancellationToken);
            var transactionHash = await _walletBlockchainClient.SendEthereumAsync(wallet.PrivateKey, recipientAddress, amount, Chain.Sepolia, cancellationToken);

            _logger.LogInformation("ETH transfer succeeded. TxHash: {TransactionHash} From user: {UserId} To: {RecipientAddress} Amount: {Amount}", transactionHash, userId, recipientAddress, amount);
            return transactionHash;
        }

        public async Task<string> GetTransactionDetails(string transactionId, CancellationToken cancellationToken = default)
        {
            var result = await _walletBlockchainClient.GetTransactionDetailsAsync(transactionId, cancellationToken);
            _logger.LogInformation("Transaction details retrieved for transaction {TransactionId}: {TransactionDetails}", transactionId, result);
            return result;
        }
    }
}
