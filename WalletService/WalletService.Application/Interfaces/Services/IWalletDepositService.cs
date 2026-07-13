namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletDepositService
    {
        /// <summary>
        /// Credits a detected on-chain deposit to the owning user's ledger. Idempotent by transaction
        /// hash and ignores addresses that don't belong to this exchange.
        /// </summary>
        /// <returns>True if the deposit was credited; false if it was ignored (unknown address or duplicate).</returns>
        Task<bool> ProcessDepositAsync(string toAddress, string assetSymbol, decimal amount, string transactionHash, CancellationToken cancellationToken = default);
    }
}
