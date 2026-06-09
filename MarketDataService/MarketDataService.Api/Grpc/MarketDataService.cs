using Grpc.Core;
using MarketDataService.Api.Protos;

namespace MarketDataService.Api.Grpc
{
    public class MarketDataService : TradingPairGrpc.TradingPairGrpcBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context) {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }
    }
}
