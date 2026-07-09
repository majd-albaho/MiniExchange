namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletFundService
    {
        Task LockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);
        Task UnlockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);

        /// <summary>Dev/testing only: credits a balance with no real funds movement behind it.</summary>
        Task CreditFund(Guid userId, string assetName, decimal amount, CancellationToken cancellationToken = default);
    }
}
