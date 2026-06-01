using System.Globalization;
using Grpc.Core;
using WalletService.Api.Protos;
using WalletService.Application.Interfaces.Services;

namespace WalletService.Api.Grpc
{
    public class WalletService : TradingPairGrpc.TradingPairGrpcBase
    {
        private readonly IUserWalletService _userWalletService;

        public WalletService(IUserWalletService userWalletService) {
            _userWalletService = userWalletService;
        }

        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context) {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }

        public override async Task<FundLockGrpcResponse> LockFund(FundLockGrpcRequest request, ServerCallContext context) {
            var (userId, amount) = ParseFundLockRequest(request);
            var lockedBalance = await _userWalletService.LockFund(userId, amount, context.CancellationToken);
            return new FundLockGrpcResponse { LockedBalance = lockedBalance.ToString(CultureInfo.InvariantCulture) };
        }

        public override async Task<FundLockGrpcResponse> UnlockFund(FundLockGrpcRequest request, ServerCallContext context) {
            var (userId, amount) = ParseFundLockRequest(request);
            var lockedBalance = await _userWalletService.UnlockFund(userId, amount, context.CancellationToken);
            return new FundLockGrpcResponse { LockedBalance = lockedBalance.ToString(CultureInfo.InvariantCulture) };
        }

        private static (Guid UserId, decimal Amount) ParseFundLockRequest(FundLockGrpcRequest request) {
            if (!Guid.TryParse(request.UserId, out var userId)) {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "user_id must be a valid GUID"));
            }

            if (!decimal.TryParse(request.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)) {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "amount must be a valid decimal value"));
            }

            return (userId, amount);
        }
    }
}
