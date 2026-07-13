namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletTransactionService
    {
        /// <summary>
        /// Withdraws a real on-chain asset to an external address. Demo/test tokens are rejected —
        /// only real on-chain assets (ETH) can be sent.
        /// </summary>
        Task<string> Send(Guid userId, string assetSymbol, string recipientAddress, decimal amount, CancellationToken cancellationToken = default);
        Task<string> GetTransactionDetails(string transactionId, CancellationToken cancellationToken = default);
    }
}
