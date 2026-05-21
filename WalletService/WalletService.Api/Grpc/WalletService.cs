using Grpc.Core;
using WalletService.Api.Protos;

namespace WalletService.Api.Grpc
{
    public class WalletService : TradingPairGrpc.TradingPairGrpcBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context) {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }
    }
}
