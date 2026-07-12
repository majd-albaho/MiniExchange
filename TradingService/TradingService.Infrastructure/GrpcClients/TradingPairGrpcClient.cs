using System.Globalization;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Clients;
using TradingService.Infrastructure.Protos;

namespace TradingService.Infrastructure.GrpcClients
{
    public sealed class TradingPairGrpcClient : ITradingPairClient
    {
        private readonly TradingPairGrpc.TradingPairGrpcClient _client;

        public TradingPairGrpcClient(TradingPairGrpc.TradingPairGrpcClient client)
        {
            _client = client;
        }

        public async Task<TradingPairInfo?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetBySymbolAsync(
                new GetTradingPairRequest { Symbol = symbol },
                cancellationToken: cancellationToken);

            if (!response.Found)
            {
                return null;
            }

            return new TradingPairInfo
            {
                Symbol = response.Symbol,
                BaseAsset = response.BaseAsset,
                QuoteAsset = response.QuoteAsset,
                MinOrderQuantity = decimal.Parse(response.MinOrderQuantity, CultureInfo.InvariantCulture),
                MinOrderValue = decimal.Parse(response.MinOrderValue, CultureInfo.InvariantCulture),
                PricePrecision = response.PricePrecision,
                QuantityPrecision = response.QuantityPrecision,
                IsActive = response.IsActive
            };
        }
    }
}
