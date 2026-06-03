namespace WalletService.Application.Interfaces.Services
{
    public interface IWalletFundService
    {
        Task LockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);
        Task UnlockFund(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);
    }
}
