using Grpc.Core;
using System.Globalization;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Services;
using TradingService.Domain.Entities;
using TradingService.Api.Protos;

namespace TradingService.Api.Grpc
{
    public class TradingService : TradingPairGrpc.TradingPairGrpcBase
    {
        private readonly IOrderService _orders;

        public TradingService(IOrderService orders)
        {
            _orders = orders;
        }

        public override async Task<OrderGrpcResponse> GetOrderById(OrderByIdGrpcRequest request, ServerCallContext context)
        {
            var id = ParseGuid(request.Id, "id");
            var order = await _orders.GetByIdAsync(id, context.CancellationToken);
            if (order is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Order was not found."));
            }

            return Map(order);
        }

        public override async Task<OrderGrpcResponse> CreateOrder(CreateOrderGrpcRequest request, ServerCallContext context)
        {
            var createRequest = new CreateOrderRequest
            {
                UserId = ParseGuid(request.UserId, "user_id"),
                PairSymbol = request.PairSymbol,
                Side = (OrderSide)(int)request.Side,
                Type = (OrderType)(int)request.Type,
                Price = ParseDecimal(request.Price, "price"),
                Quantity = ParseDecimal(request.Quantity, "quantity"),
                CreatedBy = request.CreatedBy
            };

            try
            {
                return Map(await _orders.CreateAsync(createRequest, context.CancellationToken));
            }
            catch (ArgumentException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
        }

        public override async Task<RemoveOrderGrpcResponse> RemoveOrder(OrderByIdGrpcRequest request, ServerCallContext context)
        {
            var id = ParseGuid(request.Id, "id");
            var removed = await _orders.DeleteAsync(id, "Grpc", context.CancellationToken);
            return new RemoveOrderGrpcResponse { Removed = removed };
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

        private static OrderGrpcResponse Map(OrderResponse order)
        {
            return new OrderGrpcResponse
            {
                Id = order.Id.ToString(),
                UserId = order.UserId.ToString(),
                PairSymbol = order.PairSymbol,
                Side = (OrderSideGrpc)(int)order.Side,
                Type = (OrderTypeGrpc)(int)order.Type,
                Price = order.Price.ToString(CultureInfo.InvariantCulture),
                Quantity = order.Quantity.ToString(CultureInfo.InvariantCulture),
                FilledQuantity = order.FilledQuantity.ToString(CultureInfo.InvariantCulture),
                Status = (OrderStatusGrpc)(int)order.Status,
                CreatedDate = order.CreatedDate.ToString("O", CultureInfo.InvariantCulture),
                ModifiedDate = order.ModifiedDate.ToString("O", CultureInfo.InvariantCulture)
            };
        }
    }
}
