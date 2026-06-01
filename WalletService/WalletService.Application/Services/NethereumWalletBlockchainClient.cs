using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using WalletService.Application.Interfaces.ExternalServices;

namespace WalletService.Application.Services
{
    internal sealed class NethereumWalletBlockchainClient : IWalletBlockchainClient
    {
        private const string AlchemyApiUrl = "https://eth-sepolia.g.alchemy.com/v2/7tloHtXeoED-phvbnG5Fe";

        public async Task<decimal> GetEtherBalanceAsync(string address, CancellationToken cancellationToken = default) {
            var web3 = new Web3(AlchemyApiUrl);
            var weiBalance = await web3.Eth.GetBalance.SendRequestAsync(address);
            return Web3.Convert.FromWei(weiBalance.Value);
        }

        public async Task<string> SendEtheriumAsync(string privateKey, string recipientAddress, decimal amount, Chain chain, CancellationToken cancellationToken = default) {
            var account = new Account(privateKey, chain);
            var web3 = new Web3(account, AlchemyApiUrl);

            var transactionReceipt = await web3.Eth.GetEtherTransferService()
                .TransferEtherAndWaitForReceiptAsync(recipientAddress, amount);

            if (transactionReceipt.Status.Value != 1) {
                throw new Exception("ETH transfer failed");
            }

            return transactionReceipt.TransactionHash;
        }

        public async Task<string> GetTransactionDetailsAsync(string transactionId, CancellationToken cancellationToken = default) {
            var web3 = new Web3(AlchemyApiUrl);

            var tx = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionId);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionId);
            if (receipt == null) {
                return $"Transaction with hash {transactionId} is still pending or does not exist.";
            }

            return $"Transaction Hash: {tx.TransactionHash}, " +
                   $"From: {tx.From}, To: {tx.To}, " +
                   $"Status: {(receipt.Status.Value == 1 ? "Success" : "Failure")}, " +
                   $"Value: {Web3.Convert.FromWei(tx.Value.Value)} ETH, " +
                   $"BlockNumber: {receipt.BlockNumber.Value}, " +
                   $"GasUsed: {receipt.GasUsed.Value} gas, " +
                   $"EffectiveGasPrice: {Web3.Convert.FromWei(receipt.EffectiveGasPrice.Value)} ETH";
        }
    }
}
