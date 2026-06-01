using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using Nethereum.Web3.Accounts;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Entities;

namespace WalletService.Application.Services
{
    public class UserWalletService : IUserWalletService
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IWalletBlockchainClient _walletBlockchainClient;
        private readonly ILogger<UserWalletService> _logger;

        public UserWalletService(IUserWalletRepository userWalletRepository, IWalletBlockchainClient walletBlockchainClient, ILogger<UserWalletService> logger) {
            _userWalletRepository = userWalletRepository;
            _walletBlockchainClient = walletBlockchainClient;
            _logger = logger;
        }

        public async Task<decimal> CheckBalance(Guid userId) {
            var wallet = await GetUserWallet(userId);
            return await _walletBlockchainClient.GetEtherBalanceAsync(wallet.Address);
        }

        public async Task<UserWallet> GetUserWallet(Guid userId) {
            var wallet = await _userWalletRepository.GetByUserIdAsync(userId);
            if (wallet != null)
                return wallet;

            _logger?.LogInformation($"No wallet found for user {userId}. Creating new wallet.");
            wallet = CreateWallet(userId);
            await _userWalletRepository.CreateAsync(wallet);
            _logger?.LogInformation($"Created new wallet for user {userId} with address {wallet.Address}");

            return wallet;
        }

        public async Task<decimal> LockFund(Guid userId, decimal amount, CancellationToken cancellationToken = default) {
            EnsurePositiveAmount(amount);

            var wallet = await GetUserWallet(userId);
            var totalBalance = await _walletBlockchainClient.GetEtherBalanceAsync(wallet.Address, cancellationToken);
            if (totalBalance < amount) {
                throw new InvalidOperationException("Insufficient balance to lock funds");
            }

            var locked = await _userWalletRepository.TryLockFundsAsync(wallet.Id, amount, totalBalance, userId.ToString(), cancellationToken);
            if (!locked) {
                throw new InvalidOperationException("Insufficient available balance to lock funds");
            }

            var updatedWallet = await _userWalletRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Wallet not found after locking funds");

            _logger?.LogInformation($"Locked {amount} ETH for user {userId}. Locked balance is {updatedWallet.LockedBalance} ETH.");
            return updatedWallet.LockedBalance;
        }

        public async Task<decimal> UnlockFund(Guid userId, decimal amount, CancellationToken cancellationToken = default) {
            EnsurePositiveAmount(amount);

            var wallet = await _userWalletRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wallet == null) {
                throw new InvalidOperationException("Wallet not found");
            }

            var unlocked = await _userWalletRepository.TryUnlockFundsAsync(wallet.Id, amount, userId.ToString(), cancellationToken);
            if (!unlocked) {
                throw new InvalidOperationException("Insufficient locked balance to unlock funds");
            }

            var updatedWallet = await _userWalletRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Wallet not found after unlocking funds");

            _logger?.LogInformation($"Unlocked {amount} ETH for user {userId}. Locked balance is {updatedWallet.LockedBalance} ETH.");
            return updatedWallet.LockedBalance;
        }

        public async Task<string> SendEtherium(Guid userId, string recipientAddress, decimal amount) {
            _logger?.LogInformation($"Initiating transfer of {amount} ETH from user {userId} to {recipientAddress}");

            var wallet = await GetUserWallet(userId);
            var transactionHash = await _walletBlockchainClient.SendEtheriumAsync(wallet.PrivateKey, recipientAddress, amount, Chain.Sepolia);

            _logger?.LogInformation($"ETH transfer succeeded. TxHash: {transactionHash} From user: {userId} To: {recipientAddress} Amount: {amount}");
            return transactionHash;
        }

        public async Task<string> GetTransactionDetails(string transactionId) {
            var result = await _walletBlockchainClient.GetTransactionDetailsAsync(transactionId);
            _logger?.LogInformation(result);
            return result;
        }

        private static void EnsurePositiveAmount(decimal amount) {
            if (amount <= 0) {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero");
            }
        }

        private UserWallet CreateWallet(Guid userId) {
            var ecKey = EthECKey.GenerateKey();

            var privateKey = ecKey.GetPrivateKey();
            var address = ecKey.GetPublicAddress();

            return new UserWallet {
                Id = default,
                UserId = userId,
                Address = address,
                PrivateKey = privateKey,
                LockedBalance = 0m,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedBy = userId.ToString(),
                ModifiedDate = DateTimeOffset.UtcNow,
            };
        }
    }
}
