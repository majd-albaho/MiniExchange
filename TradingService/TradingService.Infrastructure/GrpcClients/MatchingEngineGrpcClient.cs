using System.Globalization;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Clients;
using TradingService.Domain.Entities;
using TradingService.Infrastructure.Protos;

namespace TradingService.Infrastructure.GrpcClients
{
    public sealed class MatchingEngineGrpcClient : IMatchingEngineClient
    {
        private readonly MatchingEngineGrpc.MatchingEngineGrpcClient _client;

        public MatchingEngineGrpcClient(MatchingEngineGrpc.MatchingEngineGrpcClient client)
        {
            _client = client;
        }

        public async Task<bool> SubmitOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            var request = new SubmitOrderRequest
            {
                OrderId = order.Id.ToString(),
                PairSymbol = order.PairSymbol,
                Side = (OrderSideGrpc)(int)order.Side,
                Type = (OrderTypeGrpc)(int)order.Type,
                Price = order.Price.ToString(CultureInfo.InvariantCulture),
                Quantity = order.Quantity.ToString(CultureInfo.InvariantCulture)
            };

            var response = await _client.SubmitOrderAsync(request, cancellationToken: cancellationToken);
            return response.Accepted;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string pairSymbol, CancellationToken cancellationToken = default)
        {
            var request = new CancelOrderRequest
            {
                OrderId = orderId.ToString(),
                PairSymbol = pairSymbol
            };

            var response = await _client.CancelOrderAsync(request, cancellationToken: cancellationToken);
            return response.Canceled;
        }

        public async Task<OrderBookSnapshotDto> GetOrderBookAsync(string pairSymbol, int depth, CancellationToken cancellationToken = default)
        {
            var request = new GetOrderBookRequest { PairSymbol = pairSymbol, Depth = depth };
            var response = await _client.GetOrderBookAsync(request, cancellationToken: cancellationToken);

            return new OrderBookSnapshotDto
            {
                PairSymbol = response.PairSymbol,
                Bids = response.Bids.Select(ToLevel).ToList(),
                Asks = response.Asks.Select(ToLevel).ToList()
            };
        }

        private static OrderBookLevelDto ToLevel(OrderBookLevelGrpc level)
        {
            return new OrderBookLevelDto
            {
                Price = decimal.Parse(level.Price, CultureInfo.InvariantCulture),
                Quantity = decimal.Parse(level.Quantity, CultureInfo.InvariantCulture)
            };
        }
    }
}
