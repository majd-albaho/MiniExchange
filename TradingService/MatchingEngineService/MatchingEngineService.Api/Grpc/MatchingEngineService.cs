using Grpc.Core;
using MatchingEngineService.Api.Protos;

namespace MatchingEngineService.Api.Grpc
{
    public class MatchingEngineService : TradingPairGrpc.TradingPairGrpcBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context) {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }
    }
}
