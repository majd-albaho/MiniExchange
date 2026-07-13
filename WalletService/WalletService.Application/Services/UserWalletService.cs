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
        private readonly IWalletBlockchainClient _walletBlockchainClient;
        private readonly ILogger<UserWalletService> _logger;

        public UserWalletService(
            IUserWalletRepository userWalletRepository,
            IUserWalletAddressRepository userWalletAddressRepository,
            IWalletBlockchainClient walletBlockchainClient,
            ILogger<UserWalletService> logger)
        {
            _userWalletRepository = userWalletRepository;
            _userWalletAddressRepository = userWalletAddressRepository;
            _walletBlockchainClient = walletBlockchainClient;
            _logger = logger;
        }

        public async Task<UserWalletDto> GetUserWallet(Guid userId, CancellationToken cancellationToken = default)
        {
            var wallet = await _userWalletRepository.GetByUserIdAsync(userId, cancellationToken);
            if (wallet == null)
            {
                _logger.LogInformation("No wallet found for user {UserId}. Creating new wallet.", userId);
                wallet = await CreateWalletAsync(userId, cancellationToken);

                _logger.LogInformation("Created new wallet for user {UserId}", userId);
            }

            return new UserWalletDto
            {
                Id = wallet.Id,
                UserId = userId,
                WalletName = wallet.WalletName
            };
        }

        public async Task<UserWalletAddress> GetUserWalletAddress(Guid userId, CryptoNetworkType networkType, CancellationToken cancellationToken = default)
        {
            if (networkType != CryptoNetworkType.Ethereum)
                throw new NotSupportedException($"{networkType} wallet addresses are not supported yet");
            var userWallet = await GetUserWallet(userId, cancellationToken);
            var walletAddress = await _userWalletAddressRepository.GetByUserWalletId(userWallet.Id, networkType, cancellationToken);
            if (walletAddress == null)
            {
                _logger.LogInformation("No wallet address found for user {UserId}. Generating {NetworkType} address.", userId, networkType);
                walletAddress = await CreateWalletAddressAsync(userWallet.Id, networkType, cancellationToken);

                _logger.LogInformation("Created {NetworkType} wallet address for user {UserId}", networkType, userId);
            }

            return walletAddress;
        }

        public async Task<decimal> CheckEthereumBalance(Guid userId, CancellationToken cancellationToken = default)
        {
            var wallet = await GetUserWalletAddress(userId, CryptoNetworkType.Ethereum, cancellationToken);
            return await _walletBlockchainClient.GetEthereumBalanceAsync(wallet.PublicAddress, cancellationToken);
        }

        public async Task<Guid?> ResolveUserIdByAddressAsync(string publicAddress, CancellationToken cancellationToken = default)
        {
            var address = await _userWalletAddressRepository.GetByPublicAddressAsync(publicAddress, cancellationToken);
            if (address is null)
                return null;

            var wallet = await _userWalletRepository.GetByIdAsync(address.UserWalletId, cancellationToken);
            return wallet?.UserId;
        }

        private Task<UserWallet> CreateWalletAsync(Guid userId, CancellationToken cancellationToken)
        {
            var userWallet = new UserWallet
            {
                Id = default,
                UserId = userId,
                WalletName = $"User {userId} Wallet",
                CreatedBy = userId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            };
            return _userWalletRepository.CreateAsync(userWallet, cancellationToken);
        }

        private Task<UserWalletAddress> CreateWalletAddressAsync(long userWalletId, CryptoNetworkType networkType, CancellationToken cancellationToken)
        {
            var ecKey = EthECKey.GenerateKey();

            var userWalletAddress = new UserWalletAddress
            {
                Id = default,
                UserWalletId = userWalletId,
                CryptoNetworkType = networkType,
                PublicAddress = ecKey.GetPublicAddress(),
                PrivateKey = ecKey.GetPrivateKey(),
                CreatedBy = userWalletId.ToString(),
                CreatedDate = DateTimeOffset.UtcNow
            };
            return _userWalletAddressRepository.AddAsync(userWalletAddress, cancellationToken);
        }
    }
}
