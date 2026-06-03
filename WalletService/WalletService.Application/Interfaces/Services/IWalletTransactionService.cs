namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletTransactionService
    {
        Task<string> SendEthereum(Guid userId, string recipientAddress, decimal amount, CancellationToken cancellationToken = default);
        Task<string> GetTransactionDetails(string transactionId, CancellationToken cancellationToken = default);
    }
}
