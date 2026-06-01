using Nethereum.Signer;

namespace WalletService.Application.Interfaces.ExternalServices
{
    public interface IWalletBlockchainClient
    {
        Task<decimal> GetEtherBalanceAsync(string address, CancellationToken cancellationToken = default);
        Task<string> SendEtheriumAsync(string privateKey, string recipientAddress, decimal amount, Chain chain, CancellationToken cancellationToken = default);
        Task<string> GetTransactionDetailsAsync(string transactionId, CancellationToken cancellationToken = default);
    }
}
