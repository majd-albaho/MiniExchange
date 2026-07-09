using Microsoft.Extensions.Logging;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Clients;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Application.Interfaces.Services;
using TradingService.Domain.Entities;

namespace TradingService.Application.Services
{
    public sealed class OrderService : IOrderService
    {
        private const string SystemActor = "TradingService";
        private readonly IOrderRepository _orders;
        private readonly IMatchingEngineClient _matchingEngine;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository orders, IMatchingEngineClient matchingEngine, ILogger<OrderService> logger)
        {
            _orders = orders;
            _matchingEngine = matchingEngine;
            _logger = logger;
        }

        public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Order id is required.", nameof(id));
            }

            var order = await _orders.GetByIdAsync(id, cancellationToken);
            return order is null ? null : Map(order);
        }

        public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var pairSymbol = NormalizePairSymbol(request.PairSymbol);
            ValidateCreateRequest(request, pairSymbol);

            var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? SystemActor : request.CreatedBy.Trim();
            var now = DateTimeOffset.UtcNow;
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PairSymbol = pairSymbol,
                Side = request.Side,
                Type = request.Type,
                Price = request.Price,
                Quantity = request.Quantity,
                FilledQuantity = 0m,
                Status = OrderStatus.Pending,
                CreatedDate = now,
                CreatedBy = createdBy,
                ModifiedDate = now,
                ModifiedBy = createdBy
            };

            var created = await _orders.CreateAsync(order, cancellationToken);

            try
            {
                await _matchingEngine.SubmitOrderAsync(created, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit order {OrderId} to the matching engine. It remains Pending.", created.Id);
            }

            return Map(created);
        }

        public async Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Order id is required.", nameof(id));
            }

            var order = await _orders.GetByIdAsync(id, cancellationToken);
            if (order is null)
            {
                return false;
            }

            var actor = string.IsNullOrWhiteSpace(deletedBy) ? SystemActor : deletedBy.Trim();
            var deleted = await _orders.DeleteAsync(id, actor, cancellationToken);

            if (deleted)
            {
                try
                {
                    await _matchingEngine.CancelOrderAsync(id, order.PairSymbol, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cancel order {OrderId} on the matching engine.", id);
                }
            }

            return deleted;
        }

        public async Task<IReadOnlyList<OrderResponse>> GetOpenByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User id is required.", nameof(userId));
            }

            var orders = await _orders.GetOpenByUserAsync(userId, cancellationToken);
            return orders.Select(Map).ToList();
        }

        private static string NormalizePairSymbol(string pairSymbol)
        {
            if (string.IsNullOrWhiteSpace(pairSymbol))
            {
                throw new ArgumentException("Pair symbol is required.", nameof(pairSymbol));
            }

            return pairSymbol.Trim().ToUpperInvariant();
        }

        private static void ValidateCreateRequest(CreateOrderRequest request, string pairSymbol)
        {
            if (request.UserId == Guid.Empty)
            {
                throw new ArgumentException("User id is required.", nameof(request));
            }

            if (pairSymbol.Length > 20)
            {
                throw new ArgumentException("Pair symbol cannot exceed 20 characters.", nameof(request));
            }

            if (request.Side is not OrderSide.Buy and not OrderSide.Sell)
            {
                throw new ArgumentException("Order side must be Buy or Sell.", nameof(request));
            }

            if (request.Type is not OrderType.Market and not OrderType.Limit)
            {
                throw new ArgumentException("Order type must be Market or Limit.", nameof(request));
            }

            if (request.Quantity <= 0m)
            {
                throw new ArgumentException("Quantity must be greater than zero.", nameof(request));
            }

            if (request.Type == OrderType.Limit && request.Price <= 0m)
            {
                throw new ArgumentException("Limit order price must be greater than zero.", nameof(request));
            }

            if (request.Type == OrderType.Market && request.Price < 0m)
            {
                throw new ArgumentException("Market order price cannot be negative.", nameof(request));
            }
        }

        private static OrderResponse Map(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                PairSymbol = order.PairSymbol,
                Side = order.Side,
                Type = order.Type,
                Price = order.Price,
                Quantity = order.Quantity,
                FilledQuantity = order.FilledQuantity,
                Status = order.Status,
                CreatedDate = order.CreatedDate,
                ModifiedDate = order.ModifiedDate
            };
        }
    }
}
