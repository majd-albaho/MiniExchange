using System.Globalization;
using TradingService.Application.Interfaces.Clients;
using TradingService.Infrastructure.Protos;

namespace TradingService.Infrastructure.GrpcClients
{
    public sealed class WalletServiceGrpcClient : IWalletServiceClient
    {
        private readonly WalletGrpc.WalletGrpcClient _client;

        public WalletServiceGrpcClient(WalletGrpc.WalletGrpcClient client)
        {
            _client = client;
        }

        public async Task<long> GetOrCreateAssetAsync(string assetName, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetOrCreateAssetAsync(
                new GetOrCreateAssetRequest { AssetName = assetName },
                cancellationToken: cancellationToken);

            return long.Parse(response.AssetId, CultureInfo.InvariantCulture);
        }

        public async Task LockFundsAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
        {
            await _client.LockFundAsync(BuildFundLockRequest(userId, assetId, amount), cancellationToken: cancellationToken);
        }

        public async Task UnlockFundsAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken = default)
        {
            await _client.UnlockFundAsync(BuildFundLockRequest(userId, assetId, amount), cancellationToken: cancellationToken);
        }

        public async Task<bool> SettleTradeAsync(
            Guid tradeId,
            Guid buyerUserId,
            Guid sellerUserId,
            long baseAssetId,
            long quoteAssetId,
            decimal quantity,
            decimal price,
            CancellationToken cancellationToken = default)
        {
            var request = new SettleTradeRequest
            {
                TradeId = tradeId.ToString(),
                BuyerUserId = buyerUserId.ToString(),
                SellerUserId = sellerUserId.ToString(),
                BaseAssetId = baseAssetId.ToString(CultureInfo.InvariantCulture),
                QuoteAssetId = quoteAssetId.ToString(CultureInfo.InvariantCulture),
                Quantity = quantity.ToString(CultureInfo.InvariantCulture),
                Price = price.ToString(CultureInfo.InvariantCulture)
            };

            var response = await _client.SettleTradeAsync(request, cancellationToken: cancellationToken);
            return response.Success;
        }

        private static FundLockGrpcRequest BuildFundLockRequest(Guid userId, long assetId, decimal amount)
        {
            return new FundLockGrpcRequest
            {
                UserId = userId.ToString(),
                AssetId = assetId.ToString(CultureInfo.InvariantCulture),
                Amount = amount.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}
