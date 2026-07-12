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

        /// <summary>
        /// Market buy orders don't know their exact fill price up front, so the quote-asset lock is
        /// sized off the last live price plus this buffer to tolerate slippage. Any unused portion
        /// is unlocked once the order's actual fills are known (see MatchingEngineService's
        /// FilledQuantity response and TradeSettlementService).
        /// </summary>
        private const decimal MarketBuyPriceBuffer = 1.02m;

        private readonly IOrderRepository _orders;
        private readonly IMatchingEngineClient _matchingEngine;
        private readonly ITradingPairClient _tradingPairClient;
        private readonly IWalletServiceClient _walletServiceClient;
        private readonly IMarketDataClient _marketDataClient;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orders,
            IMatchingEngineClient matchingEngine,
            ITradingPairClient tradingPairClient,
            IWalletServiceClient walletServiceClient,
            IMarketDataClient marketDataClient,
            ILogger<OrderService> logger)
        {
            _orders = orders;
            _matchingEngine = matchingEngine;
            _tradingPairClient = tradingPairClient;
            _walletServiceClient = walletServiceClient;
            _marketDataClient = marketDataClient;
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
            ValidateBasicRequest(request);

            var pair = await _tradingPairClient.GetBySymbolAsync(pairSymbol, cancellationToken)
                ?? throw new ArgumentException($"Trading pair '{pairSymbol}' does not exist.", nameof(request));

            if (!pair.IsActive)
            {
                throw new ArgumentException($"Trading pair '{pairSymbol}' is not currently active.", nameof(request));
            }

            var quantity = Math.Round(request.Quantity, pair.QuantityPrecision, MidpointRounding.ToEven);
            if (quantity < pair.MinOrderQuantity)
            {
                throw new ArgumentException($"Quantity must be at least {pair.MinOrderQuantity} {pair.BaseAsset} for {pairSymbol}.", nameof(request));
            }

            decimal? referencePrice = null;
            if (request.Type == OrderType.Market)
            {
                referencePrice = await _marketDataClient.GetLatestPriceAsync(pairSymbol, cancellationToken);
                if (referencePrice is null or <= 0m)
                {
                    throw new InvalidOperationException($"No live price is available yet for {pairSymbol}. Try again shortly or place a limit order.");
                }
            }

            var price = request.Type == OrderType.Limit
                ? Math.Round(request.Price, pair.PricePrecision, MidpointRounding.ToEven)
                : referencePrice!.Value;

            if (request.Type == OrderType.Limit && price <= 0m)
            {
                throw new ArgumentException("Limit order price must be greater than zero.", nameof(request));
            }

            if (quantity * price < pair.MinOrderValue)
            {
                throw new ArgumentException($"Order value must be at least {pair.MinOrderValue} {pair.QuoteAsset} for {pairSymbol}.", nameof(request));
            }

            var baseAssetId = await _walletServiceClient.GetOrCreateAssetAsync(pair.BaseAsset, cancellationToken);
            var quoteAssetId = await _walletServiceClient.GetOrCreateAssetAsync(pair.QuoteAsset, cancellationToken);

            var lockAssetId = request.Side == OrderSide.Sell ? baseAssetId : quoteAssetId;
            var lockAmount = request.Side == OrderSide.Sell
                ? quantity
                : request.Type == OrderType.Limit
                    ? quantity * price
                    : quantity * referencePrice!.Value * MarketBuyPriceBuffer;

            await _walletServiceClient.LockFundsAsync(request.UserId, lockAssetId, lockAmount, cancellationToken);

            var createdBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? SystemActor : request.CreatedBy.Trim();
            var now = DateTimeOffset.UtcNow;
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                PairSymbol = pairSymbol,
                Side = request.Side,
                Type = request.Type,
                Price = price,
                Quantity = quantity,
                FilledQuantity = 0m,
                Status = OrderStatus.Pending,
                BaseAssetId = baseAssetId,
                QuoteAssetId = quoteAssetId,
                LockedAmount = lockAmount,
                CreatedDate = now,
                CreatedBy = createdBy,
                ModifiedDate = now,
                ModifiedBy = createdBy
            };

            Order created;
            try
            {
                created = await _orders.CreateAsync(order, cancellationToken);
            }
            catch
            {
                await TryUnlockAsync(request.UserId, lockAssetId, lockAmount, cancellationToken);
                throw;
            }

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
                await TryUnlockRemainingAsync(order, cancellationToken);

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

        private async Task TryUnlockAsync(Guid userId, long assetId, decimal amount, CancellationToken cancellationToken)
        {
            try
            {
                await _walletServiceClient.UnlockFundsAsync(userId, assetId, amount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to roll back a wallet lock of {Amount} for asset {AssetId} after order persistence failed.", amount, assetId);
            }
        }

        private async Task TryUnlockRemainingAsync(Order order, CancellationToken cancellationToken)
        {
            if (order.LockedAmount <= 0m || order.BaseAssetId is null || order.QuoteAssetId is null)
            {
                return;
            }

            var lockAssetId = order.Side == OrderSide.Sell ? order.BaseAssetId.Value : order.QuoteAssetId.Value;

            try
            {
                await _walletServiceClient.UnlockFundsAsync(order.UserId, lockAssetId, order.LockedAmount, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unlock the remaining reserved balance for cancelled order {OrderId}.", order.Id);
            }
        }

        private static string NormalizePairSymbol(string pairSymbol)
        {
            if (string.IsNullOrWhiteSpace(pairSymbol))
            {
                throw new ArgumentException("Pair symbol is required.", nameof(pairSymbol));
            }

            return pairSymbol.Trim().ToUpperInvariant();
        }

        private static void ValidateBasicRequest(CreateOrderRequest request)
        {
            if (request.UserId == Guid.Empty)
            {
                throw new ArgumentException("User id is required.", nameof(request));
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
