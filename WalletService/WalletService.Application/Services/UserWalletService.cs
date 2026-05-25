using Nethereum.Hex.HexTypes;
using Nethereum.Web3.Accounts;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Interfaces.Services;

namespace WalletService.Application.Services
{
    internal class UserWalletService : IUserWalletService
    {
        private readonly IUserWalletRepository _userWalletRepository;

        public UserWalletService(IUserWalletRepository userWalletRepository) {
            _userWalletRepository = userWalletRepository;
        }

        public async Task<string> GetUserWallet(Guid userId) {
            var wallet = await _userWalletRepository.GetByUserIdAsync(userId);
            if (wallet != null)
                return wallet.Address;

            wallet = CreateWallet(userId);
            await _userWalletRepository.CreateAsync(wallet);

            return wallet.Address;
        }

        private Account LoadWallet(Guid userId) {
            var wallet = _userWalletRepository.GetByUserIdAsync(userId).Result;
            if (wallet == null)
                throw new Exception("Wallet not found");

            string privateKey = wallet.PrivateKey;
            var account = new Account(privateKey);
            return account;
        }

        public async Task<HexBigInteger> CheckBalance(Guid userId) {
            var account = LoadWallet(userId);
            var web3 = new Nethereum.Web3.Web3(account);
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            Console.WriteLine($"Balance: {balance.Value.ToString()}");

            return balance;

            //var web3 = new Web3("https://sepolia.infura.io/v3/YOUR_KEY");
            //var balanceWei = await web3.Eth.GetBalance.SendRequestAsync(account.Address);
            //decimal balanceEth = Web3.Convert.FromWei(balanceWei);
            //Console.WriteLine($"Balance: {balanceEth} ETH");
        }

        private async Task SendEtherium(Guid userId, string recipientAddress, decimal amount) {
            //https://www.alchemy.com/faucets/ethereum-sepolia?utm_source=chatgpt.com
            //https://www.alchemy.com/?utm_source=chatgpt.com

            //https://nethereum.com/?utm_source=chatgpt.com
            var account = LoadWallet(userId);
            var web3 = new Nethereum.Web3.Web3(account);
            var transactionHash = await web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(recipientAddress, amount);
            Console.WriteLine($"Transaction Hash: {transactionHash}");


            //string privateKey = "YOUR_PRIVATE_KEY";

            //var account = new Account(privateKey, Nethereum.Signer.Chain.Sepolia);
            //var web3 = new Web3(account, "https://sepolia.infura.io/v3/YOUR_KEY");

            //var receipt = await web3.Eth
            //    .GetEtherTransferService()
            //    .TransferEtherAndWaitForReceiptAsync("0xReceiverAddress", 0.001m);
            //Console.WriteLine($"TX: {receipt.TransactionHash}");
        }


        private Domain.Entities.UserWallet CreateWallet(Guid userId) {
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

            var privateKey = ecKey.GetPrivateKey();
            var address = ecKey.GetPublicAddress();

            Console.WriteLine($"Address: {address}");
            Console.WriteLine($"Private Key: 0x{privateKey}");

            return new Domain.Entities.UserWallet {
                Id = default,
                UserId = userId,
                Address = address,
                PrivateKey = privateKey,
                CreatedBy = userId.ToString(),
                CreatedDate = DateTime.UtcNow,
            };
        }
    }
}
