using Grpc.Core;
using AuthService.Api.Protos;

namespace AuthService.Api.Grpc
{
    public class AuthService : TradingPairGrpc.TradingPairGrpcBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context) {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }
    }
}
