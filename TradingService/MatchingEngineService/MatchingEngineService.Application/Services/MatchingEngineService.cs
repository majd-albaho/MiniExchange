using MatchingEngineService.Application.Dto;
using MatchingEngineService.Application.Interfaces.Services;
using MatchingEngineService.Domain.Entities;
using SharedLibrary.EventDriven;
using SharedLibrary.EventDriven.Models;

namespace MatchingEngineService.Application.Services
{
    public sealed class MatchingEngineService : IMatchingEngineService
    {
        private const string TradeExecutedQueue = "trading.trade.executed";

        private readonly OrderBookRegistry _registry;
        private readonly IMessageBroker _messageBroker;

        public MatchingEngineService(OrderBookRegistry registry, IMessageBroker messageBroker)
        {
            _registry = registry;
            _messageBroker = messageBroker;
        }

        public async Task<bool> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken cancellationToken = default)
        {
            var actor = _registry.GetOrCreate(command.PairSymbol);

            var order = new BookOrder
            {
                Id = command.OrderId,
                PairSymbol = command.PairSymbol,
                Side = command.Side,
                Type = command.Type,
                Price = command.Price,
                RemainingQuantity = command.Quantity,
                CreatedDate = DateTimeOffset.UtcNow
            };

            var trades = await actor.SubmitAsync(order);

            foreach (var trade in trades)
            {
                await _messageBroker.PublishAsync(TradeExecutedQueue, ToEvent(trade));
            }

            return true;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string pairSymbol, CancellationToken cancellationToken = default)
        {
            var actor = _registry.GetOrCreate(pairSymbol);
            return await actor.CancelAsync(orderId);
        }

        private static TradeExecutedEvent ToEvent(Trade trade)
        {
            return new TradeExecutedEvent
            {
                TradeId = trade.Id,
                PairSymbol = trade.PairSymbol,
                BuyOrderId = trade.BuyOrderId,
                SellOrderId = trade.SellOrderId,
                Price = trade.Price,
                Quantity = trade.Quantity,
                ExecutedAt = trade.ExecutedAt,
                BuyOrderRemainingQuantity = trade.BuyOrderRemainingQuantity,
                SellOrderRemainingQuantity = trade.SellOrderRemainingQuantity
            };
        }
    }
}
