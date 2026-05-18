using Grpc.Core;
using TradingPairService.Api.Protos;

namespace TradingPairService.Api.Grpc
{
    public class TradingPairService : TradingPairGrpc.TradingPairGrpcBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context) {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }
    }
}
