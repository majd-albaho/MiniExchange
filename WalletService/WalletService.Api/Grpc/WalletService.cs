using System.Globalization;
using Grpc.Core;
using WalletService.Api.Protos;
using WalletService.Application.Interfaces.Services;

namespace WalletService.Api.Grpc
{
    public class WalletService : TradingPairGrpc.TradingPairGrpcBase
    {
        private readonly IWalletFundService _walletFundService;

        public WalletService(IWalletFundService walletFundService)
        {
            _walletFundService = walletFundService;
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
    }
}
