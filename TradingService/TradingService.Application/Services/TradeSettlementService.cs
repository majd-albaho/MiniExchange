using SharedLibrary.EventDriven.Models;
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

        public TradeSettlementService(IOrderRepository orders, ITradeRepository trades)
        {
            _orders = orders;
            _trades = trades;
        }

        public async Task ApplyTradeAsync(TradeExecutedEvent trade, CancellationToken cancellationToken = default)
        {
            await ApplyToOrderAsync(trade.BuyOrderId, trade.Quantity, trade.BuyOrderRemainingQuantity, cancellationToken);
            await ApplyToOrderAsync(trade.SellOrderId, trade.Quantity, trade.SellOrderRemainingQuantity, cancellationToken);

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

        private async Task ApplyToOrderAsync(Guid orderId, decimal tradedQuantity, decimal remainingQuantity, CancellationToken cancellationToken)
        {
            var order = await _orders.GetByIdAsync(orderId, cancellationToken);
            if (order is null)
            {
                return;
            }

            order.FilledQuantity += tradedQuantity;
            order.Status = remainingQuantity <= 0 ? OrderStatus.Filled : OrderStatus.PartiallyFilled;
            order.ModifiedBy = SystemActor;

            await _orders.UpdateAsync(order, cancellationToken);
        }
    }
}
