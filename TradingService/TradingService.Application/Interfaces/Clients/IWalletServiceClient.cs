namespace TradingService.Application.Interfaces.Clients
{
    public interface IWalletServiceClient
    {
        Task<long> GetOrCreateAssetAsync(string assetName, CancellationToken cancellationToken = default);
        Task LockFundsAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);
        Task UnlockFundsAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default);

        Task<bool> SettleTradeAsync(
            Guid tradeId,
            Guid buyerUserId,
            Guid sellerUserId,
            long baseAssetId,
            long quoteAssetId,
            decimal quantity,
            decimal price,
            CancellationToken cancellationToken = default);
    }
}
