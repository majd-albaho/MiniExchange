using System.Globalization;
using Grpc.Core;
using MatchingEngineService.Api.Protos;
using MatchingEngineService.Application.Dto;
using MatchingEngineService.Application.Interfaces.Services;
using MatchingEngineService.Domain.Entities;

namespace MatchingEngineService.Api.Grpc
{
    public class MatchingEngineGrpcService : MatchingEngineGrpc.MatchingEngineGrpcBase
    {
        private readonly IMatchingEngineService _matchingEngine;

        public MatchingEngineGrpcService(IMatchingEngineService matchingEngine)
        {
            _matchingEngine = matchingEngine;
        }

        public override async Task<SubmitOrderResponse> SubmitOrder(SubmitOrderRequest request, ServerCallContext context)
        {
            var command = new SubmitOrderCommand
            {
                OrderId = ParseGuid(request.OrderId, "order_id"),
                PairSymbol = request.PairSymbol,
                Side = (OrderSide)(int)request.Side,
                Type = (OrderType)(int)request.Type,
                Price = ParseDecimal(request.Price, "price"),
                Quantity = ParseDecimal(request.Quantity, "quantity")
            };

            var accepted = await _matchingEngine.SubmitOrderAsync(command, context.CancellationToken);
            return new SubmitOrderResponse { Accepted = accepted, Message = accepted ? string.Empty : "Order was not accepted." };
        }

        public override async Task<CancelOrderResponse> CancelOrder(CancelOrderRequest request, ServerCallContext context)
        {
            var orderId = ParseGuid(request.OrderId, "order_id");
            var canceled = await _matchingEngine.CancelOrderAsync(orderId, request.PairSymbol, context.CancellationToken);
            return new CancelOrderResponse { Canceled = canceled };
        }

        public override async Task<GetOrderBookResponse> GetOrderBook(GetOrderBookRequest request, ServerCallContext context)
        {
            var depth = request.Depth <= 0 ? 20 : request.Depth;
            var snapshot = await _matchingEngine.GetOrderBookAsync(request.PairSymbol, depth, context.CancellationToken);

            var response = new GetOrderBookResponse { PairSymbol = snapshot.PairSymbol };
            response.Bids.AddRange(snapshot.Bids.Select(ToLevel));
            response.Asks.AddRange(snapshot.Asks.Select(ToLevel));
            return response;
        }

        private static OrderBookLevelGrpc ToLevel(Domain.OrderBookLevel level)
        {
            return new OrderBookLevelGrpc
            {
                Price = level.Price.ToString(CultureInfo.InvariantCulture),
                Quantity = level.Quantity.ToString(CultureInfo.InvariantCulture)
            };
        }

        private static Guid ParseGuid(string value, string fieldName)
        {
            if (!Guid.TryParse(value, out var id))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{fieldName} must be a valid GUID."));
            }

            return id;
        }

        private static decimal ParseDecimal(string value, string fieldName)
        {
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, $"{fieldName} must be a valid decimal value."));
            }

            return amount;
        }
    }
}
