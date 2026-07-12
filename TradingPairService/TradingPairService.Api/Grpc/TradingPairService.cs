using System.Globalization;
using Grpc.Core;
using TradingPairService.Api.Protos;
using TradingPairService.Application.Interfaces.Services;

namespace TradingPairService.Api.Grpc
{
    public class TradingPairService : TradingPairGrpc.TradingPairGrpcBase
    {
        private readonly ITradingPairService _tradingPairService;

        public TradingPairService(ITradingPairService tradingPairService)
        {
            _tradingPairService = tradingPairService;
        }

        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse { Message = $"Pong: {request.Message}" });
        }

        public override async Task<TradingPairGrpcResponse> GetBySymbol(GetTradingPairRequest request, ServerCallContext context)
        {
            var pair = await _tradingPairService.GetBySymbol(request.Symbol);

            if (pair is null)
            {
                return new TradingPairGrpcResponse { Found = false };
            }

            return new TradingPairGrpcResponse
            {
                Found = true,
                Symbol = pair.Symbol,
                BaseAsset = pair.BaseAsset,
                QuoteAsset = pair.QuoteAsset,
                MinOrderQuantity = pair.MinOrderQuantity.ToString(CultureInfo.InvariantCulture),
                MinOrderValue = pair.MinOrderValue.ToString(CultureInfo.InvariantCulture),
                PricePrecision = pair.PricePrecision,
                QuantityPrecision = pair.QuantityPrecision,
                IsActive = pair.IsActive
            };
        }
    }
}
