namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletFundService
    {
        Task LockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);
        Task UnlockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>Dev/testing only: credits a balance with no real funds movement behind it.</summary>
        Task CreditFund(Guid userId, string assetName, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a ledger-only demo token balance for testing. The asset is flagged <c>IsDemo</c>
        /// so it can never be withdrawn on-chain. Reserved real symbols (e.g. ETH) are rejected.
        /// </summary>
        Task AddDemoTokenAsync(Guid userId, string assetName, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>Debits the ledger and records a Withdrawal entry after an on-chain send succeeds.</summary>
        Task RecordWithdrawalAsync(Guid userId, long assetId, decimal amount, string transactionHash, CancellationToken cancellationToken = default);

        /// <summary>Credits the ledger and records a Deposit entry when a real on-chain deposit is detected.</summary>
        Task RecordDepositAsync(Guid userId, string assetName, decimal amount, string transactionHash, CancellationToken cancellationToken = default);
    }
}
