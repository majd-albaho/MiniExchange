using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using WalletService.Application.Dto;
using WalletService.Application.Interfaces.ExternalServices;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Entities;
using WalletService.Domain.Enums;

namespace WalletService.Application.Services
{
    public class UserWalletService : IUserWalletService
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly IUserWalletAddressRepository _userWalletAddressRepository;
        private readonly IUserWalletAssetsRepository _userWalletAssetsRepository;
        private readonly IWalletBlockchainClient _walletBlockchainClient;
        private readonly ILogger<UserWalletService> _logger;

        public UserWalletService(IUserWalletRepository userWalletRepository,
            IUserWalletAddressRepository userWalletAddressRepository,
            IUserWalletAssetsRepository userWalletAssetsRepository,
            IWalletBlockchainClient walletBlockchainClient, ILogger<UserWalletService> logger)
        {
            _userWalletRepository = userWalletRepository;
            _userWalletAddressRepository = userWalletAddressRepository;
            _userWalletAssetsRepository = userWalletAssetsRepository;
            _walletBlockchainClient = walletBlockchainClient;
            _logger = logger;
        }

        public async Task<UserWalletDto> GetUserWallet(Guid userId)
        {
            var wallet = await _userWalletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                _logger?.LogInformation($"No wallet found for user {userId}. Creating new wallet.");
                wallet = await CreateWalletAsync(userId);

                _logger?.LogInformation($"Created new wallet for user {userId}");
            }

            return new UserWalletDto()
            {
                Id = wallet.Id,
                UserId = userId,
                WalletName = wallet.WalletName
            };
        }

      
        public async Task<decimal> CheckEthereumBalance(Guid userId)
        {
            var wallet = await GetUserWalletAddress(userId);
            return await _walletBlockchainClient.GetEthereumBalanceAsync(wallet.PublicAddress);
        }


        public async Task LockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);
            var userWallet = await GetUserWallet(userId);
            await _userWalletAssetsRepository.LockFundsAsync(userWallet.Id, assetId, amount, userId.ToString(), cancellationToken);

            _logger?.LogInformation($"Locked {amount} of asset {assetId} for user {userId} in wallet {userWallet.Id}");
        }

        public async Task UnlockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
        {
            EnsurePositiveAmount(amount);

            var wallet = await GetUserWallet(userId);
            await _userWalletAssetsRepository.UnlockFundsAsync(wallet.Id, assetId, amount, userId.ToString(), cancellationToken);

            _logger?.LogInformation($"Unlocked {amount} of asset {assetId} for user {userId} in wallet {wallet.Id}");
        }

        public async Task<string> SendEthereum(Guid userId, string recipientAddress, decimal amount)
        {
            _logger?.LogInformation($"Initiating transfer of {amount} ETH from user {userId} to {recipientAddress}");

            var wallet = await GetUserWalletAddress(userId);
            var transactionHash = await _walletBlockchainClient.SendEthereumAsync(wallet.PrivateKey, recipientAddress, amount, Chain.Sepolia);

            _logger?.LogInformation($"ETH transfer succeeded. TxHash: {transactionHash} From user: {userId} To: {recipientAddress} Amount: {amount}");
            return transactionHash;
        }

        public async Task<string> GetTransactionDetails(string transactionId)
        {
            var result = await _walletBlockchainClient.GetTransactionDetailsAsync(transactionId);
            _logger?.LogInformation(result);
            return result;
        }

        private async Task<UserWalletAddress> GetUserWalletAddress(Guid userId)
        {
            var userWallet = await GetUserWallet(userId);
            var walletAddress = await _userWalletAddressRepository.GetByUserWalletId(userWallet.Id, CryptoNetworkType.Ethereum);
            if (walletAddress == null)
            {
                _logger?.LogInformation($"No wallet address found for user {userId}. Generating new Ethereum address.");
                walletAddress = await CreateWalletAddressAsync(userWallet.Id, CryptoNetworkType.Ethereum);

                _logger?.LogInformation($"Created new wallet for user {userId}. Generating Ethereum address.");
            }

            return walletAddress;
        }

        private static void EnsurePositiveAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero");
        }

        private Task<UserWallet> CreateWalletAsync(Guid userId)
        {
            var userWallet = new UserWallet
            {
                Id = default,
                UserId = userId,
                WalletName = $"User {userId} Wallet",
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            };
            return _userWalletRepository.CreateAsync(userWallet);
        }

        private Task<UserWalletAddress> CreateWalletAddressAsync(long userWalletId, CryptoNetworkType networkType)
        {
            var ecKey = EthECKey.GenerateKey();

            var privateKey = ecKey.GetPrivateKey();
            var publicAddress = ecKey.GetPublicAddress();

            var userWalletAddress = new UserWalletAddress
            {
                Id = default,
                UserWalletId = userWalletId,
                CryptoNetworkType = networkType,
                PublicAddress = publicAddress,
                PrivateKey = privateKey,
                CreatedBy = userWalletId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            };
            return _userWalletAddressRepository.AddAsync(userWalletAddress);
        }
    }
}

