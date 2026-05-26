using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;
using WalletService.Domain.Entities;

namespace WalletService.Application.Services
{
    internal class UserWalletService : IUserWalletService
    {
        private readonly IUserWalletRepository _userWalletRepository;
        private readonly string _alchemyApiUrl = "https://eth-sepolia.g.alchemy.com/v2/7tloHtXeoED-phvbnG5Fe";
        private readonly ILogger<UserWalletService> _logger;

        public UserWalletService(IUserWalletRepository userWalletRepository, ILogger<UserWalletService> logger) {
            _userWalletRepository = userWalletRepository;
            _logger = logger;
        }

        public async Task<decimal> CheckBalance(Guid userId) {
            var account = await LoadWallet(userId);
            var web3 = new Web3(_alchemyApiUrl);

            var weiBalance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            var balance = Web3.Convert.FromWei(BigInteger.Parse(weiBalance.Value.ToString()));

            return balance;
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

        public async Task<string> SendEtherium(Guid userId, string recipientAddress, decimal amount) {
            _logger?.LogInformation($"Initiating transfer of {amount} ETH from user {userId} to {recipientAddress}");

            var account = await LoadWallet(userId, Chain.Sepolia);
            var web3 = new Web3(account, _alchemyApiUrl);

            var transactionReceipt = await web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(recipientAddress, amount);

            var statusText = transactionReceipt.Status.Value == 1 ? "Success" : "Failure";
            _logger?.LogInformation($"ETH transfer {statusText}. " +
                   $"TxHash: {transactionReceipt.TransactionHash} " +
                   $"From: {transactionReceipt.From} " +
                   $"To: {transactionReceipt.To} " +
                   $"Amount: {amount} " +
                   $"BlockNumber: {transactionReceipt.BlockNumber.Value} " +
                   $"GasUsed: {transactionReceipt.GasUsed.Value} " +
                   $"EffectiveGasPrice: {transactionReceipt.EffectiveGasPrice.Value} " +
                   $"Status: {transactionReceipt.Status.Value}");

            if (transactionReceipt.Status.Value != 1) {
                throw new Exception("ETH transfer failed");
            }

            return transactionReceipt.TransactionHash;
        }

        public async Task<string> GetTransactionDetails(string transactionId) {
            var web3 = new Web3(_alchemyApiUrl);

            var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionId);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionId);
            if (receipt == null) {
                _logger?.LogWarning($"Transaction receipt not found for TxHash: {transactionId}. The transaction might still be pending.");
                return $"Transaction with hash {transactionId} is still pending or does not exist.";
            }

            _logger?.LogInformation($"Loading transaction details for TxHash: {transactionId}");

            var result = $"Transaction Hash: {tx.TransactionHash}, " +
                   $"From: {tx.From}, To: {tx.To}, " +
                   $"Status: {(receipt.Status.Value == 1 ? "Success" : "Failure")}, " +
                   $"Value: {Web3.Convert.FromWei(tx.Value.Value)} ETH, " +
                   $"BlockNumber: {receipt.BlockNumber.Value}, " +
                   $"GasUsed: {receipt.GasUsed.Value} gas, " +
                   $"EffectiveGasPrice: {Web3.Convert.FromWei(receipt.EffectiveGasPrice.Value)} ETH";

            _logger?.LogInformation(result);
            return result;
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
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.UtcNow,
            };
        }

        private async Task<Account> LoadWallet(Guid userId, Chain? chain = null) {
            var wallet = await GetUserWallet(userId);
            if (wallet == null) {
                _logger?.LogError($"Wallet not found for user {userId}");
                throw new Exception("Wallet not found");
            }

            if (chain != null)
                return new Account(wallet.PrivateKey, chain.Value);

            return new Account(wallet.PrivateKey);
        }
    }
}
