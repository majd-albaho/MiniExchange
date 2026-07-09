using System.Globalization;
using Grpc.Core;
using WalletService.Api.Protos;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;

namespace WalletService.Api.Grpc
{
    public class WalletGrpcService : WalletGrpc.WalletGrpcBase
    {
        private readonly IWalletFundService _walletFundService;
        private readonly IAssetService _assetService;
        private readonly IWalletSettlementService _walletSettlementService;

        public WalletGrpcService(IWalletFundService walletFundService, IAssetService assetService, IWalletSettlementService walletSettlementService)
        {
            _walletFundService = walletFundService;
            _assetService = assetService;
            _walletSettlementService = walletSettlementService;
        }

        public override async Task<FundLockGrpcResponse> LockFund(FundLockGrpcRequest request, ServerCallContext context)
        {
            var (userId, assetId, amount) = ParseFundLockRequest(request);
            await _walletFundService.LockFund(userId, assetId, amount, context.CancellationToken);
            return new FundLockGrpcResponse { LockedBalance = amount.ToString(CultureInfo.InvariantCulture) };
        }

        public override async Task<FundLockGrpcResponse> UnlockFund(FundLockGrpcRequest request, ServerCallContext context)
        {
            var (userId, assetId, amount) = ParseFundLockRequest(request);
            await _walletFundService.UnlockFund(userId, assetId, amount, context.CancellationToken);
            return new FundLockGrpcResponse { LockedBalance = amount.ToString(CultureInfo.InvariantCulture) };
        }

        public override async Task<AssetGrpcResponse> GetOrCreateAsset(GetOrCreateAssetRequest request, ServerCallContext context)
        {
            var asset = await _assetService.GetOrCreateByNameAsync(request.AssetName, context.CancellationToken);
            return new AssetGrpcResponse
            {
                AssetId = asset.Id.ToString(CultureInfo.InvariantCulture),
                AssetName = asset.AssetName
            };
        }

        public override async Task<SettleTradeResponse> SettleTrade(SettleTradeRequest request, ServerCallContext context)
        {
            var settlementRequest = ParseSettleTradeRequest(request);
            var success = await _walletSettlementService.SettleTradeAsync(settlementRequest, context.CancellationToken);
            return new SettleTradeResponse { Success = success };
        }

        private static (Guid UserId, long AssetId, decimal Amount) ParseFundLockRequest(FundLockGrpcRequest request)
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id must be a valid GUID"));
            }

            if (!long.TryParse(request.AssetId, out long assetId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "asset_id must be a valid long value"));
            }

            if (!decimal.TryParse(request.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "amount must be a valid decimal value"));
            }

            return (userId, assetId, amount);
        }

        private static TradeSettlementRequest ParseSettleTradeRequest(SettleTradeRequest request)
        {
            if (!Guid.TryParse(request.TradeId, out var tradeId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "trade_id must be a valid GUID"));
            }

            if (!Guid.TryParse(request.BuyerUserId, out var buyerUserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "buyer_user_id must be a valid GUID"));
            }

            if (!Guid.TryParse(request.SellerUserId, out var sellerUserId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "seller_user_id must be a valid GUID"));
            }

            if (!long.TryParse(request.BaseAssetId, out var baseAssetId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "base_asset_id must be a valid long value"));
            }

            if (!long.TryParse(request.QuoteAssetId, out var quoteAssetId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "quote_asset_id must be a valid long value"));
            }

            if (!decimal.TryParse(request.Quantity, NumberStyles.Number, CultureInfo.InvariantCulture, out var quantity))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "quantity must be a valid decimal value"));
            }

            if (!decimal.TryParse(request.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "price must be a valid decimal value"));
            }

            return new TradeSettlementRequest
            {
                TradeId = tradeId,
                BuyerUserId = buyerUserId,
                SellerUserId = sellerUserId,
                BaseAssetId = baseAssetId,
                QuoteAssetId = quoteAssetId,
                Quantity = quantity,
                Price = price
            };
        }
    }
}
