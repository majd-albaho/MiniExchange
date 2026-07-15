using Microsoft.Extensions.Logging;
using SharedLibrary.EventDriven.Models;
using TradingService.Application.Interfaces.Clients;
using TradingService.Application.Interfaces.Repositories;
using TradingService.Application.Interfaces.Services;
using TradingService.Domain.Entities;

namespace TradingService.Application.Services
{
    public sealed class TradeSettlementService : ITradeSettlementService
    {
        private const string SystemActor = "MatchingEngineService";

        private readonly IOrderRepository _orders;
        private readonly ITradeRepository _trades;
        private readonly IWalletServiceClient _walletServiceClient;
        private readonly ILogger<TradeSettlementService> _logger;

        public TradeSettlementService(
            IOrderRepository orders,
            ITradeRepository trades,
            IWalletServiceClient walletServiceClient,
            ILogger<TradeSettlementService> logger)
        {
            _orders = orders;
            _trades = trades;
            _walletServiceClient = walletServiceClient;
            _logger = logger;
        }

        public async Task ApplyTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default)
        {
            var buyOrder = await _orders.GetByIdAsync(trade.BuyOrderId, cancellationToken);
            var sellOrder = await _orders.GetByIdAsync(trade.SellOrderId, cancellationToken);

            // Move the money first: wallet settlement is idempotent (keyed by TradeId), so if a
            // later step fails and the event is redelivered, balances are not applied twice —
            // whereas the fill bookkeeping below is not safe to re-run after a partial failure.
            await SettleWalletsAsync(trade, buyOrder, sellOrder, cancellationToken);

            if (buyOrder is not null)
            {
                await ApplyToOrderAsync(buyOrder, trade.Quantity, trade.BuyOrderRemainingQuantity, trade.Quantity * trade.Price, cancellationToken);
            }

            if (sellOrder is not null)
            {
                await ApplyToOrderAsync(sellOrder, trade.Quantity, trade.SellOrderRemainingQuantity, trade.Quantity, cancellationToken);
            }

            var now = DateTimeOffset.UtcNow;
            await _trades.CreateAsync(new Trade
            {
                Id = trade.TradeId,
                PairSymbol = trade.PairSymbol,
                BuyOrderId = trade.BuyOrderId,
                SellOrderId = trade.SellOrderId,
                Price = trade.Price,
                Quantity = trade.Quantity,
                ExecutedAt = trade.ExecutedAt,
                CreatedDate = now,
                CreatedBy = SystemActor,
                ModifiedDate = now,
                ModifiedBy = SystemActor
            }, cancellationToken);
        }

        private async Task SettleWalletsAsync(TradeExecutedEvent trade, Order? buyOrder, Order? sellOrder, CancellationToken cancellationToken)
        {
            if (buyOrder is null || sellOrder is null)
            {
                _logger.LogWarning(
                    "Skipping wallet settlement for trade {TradeId}: buy order {BuyOrderId} or sell order {SellOrderId} was not found.",
                    trade.TradeId, trade.BuyOrderId, trade.SellOrderId);
                return;
            }

            if (buyOrder.BaseAssetId is null || buyOrder.QuoteAssetId is null)
            {
                _logger.LogWarning(
                    "Skipping wallet settlement for trade {TradeId}: order {OrderId} has no wallet asset ids.",
                    trade.TradeId, buyOrder.Id);
                return;
            }

            await _walletServiceClient.SettleTradeAsync(
                trade.TradeId,
                buyOrder.UserId,
                sellOrder.UserId,
                buyOrder.BaseAssetId.Value,
                buyOrder.QuoteAssetId.Value,
                trade.Quantity,
                trade.Price,
                cancellationToken);
        }

        private async Task ApplyToOrderAsync(Order order, decimal tradedQuantity, decimal remainingQuantity, decimal lockConsumed, CancellationToken cancellationToken)
        {
            order.FilledQuantity += tradedQuantity;
            order.Status = remainingQuantity <= 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
            order.LockedAmount = Math.Max(0m, order.LockedAmount - lockConsumed);
            order.ModifiedBy = SystemActor;

            if (order.Status == OrderStatus.Filled && order.LockedAmount > 0m)
            {
                await TryReleaseLeftoverLockAsync(order, cancellationToken);
            }

            await _orders.UpdateAsync(order, cancellationToken);
        }

        /// <summary>
        /// A buy order can fill below its limit price (and a market buy locks a slippage buffer),
        /// so once it is fully filled part of the original quote lock can be left over. Release it
        /// back to the available balance.
        /// </summary>
        private async Task TryReleaseLeftoverLockAsync(Order order, CancellationToken cancellationToken)
        {
            var lockAssetId = order.Side == OrderSide.Sell ? order.BaseAssetId : order.QuoteAssetId;
            if (lockAssetId is null)
            {
                return;
            }

            try
            {
                await _walletServiceClient.UnlockFundsAsync(order.UserId, lockAssetId.Value, order.LockedAmount, cancellationToken);
                order.LockedAmount = 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release the leftover lock of {Amount} for filled order {OrderId}.", order.LockedAmount, order.Id);
            }
        }
    }
}
