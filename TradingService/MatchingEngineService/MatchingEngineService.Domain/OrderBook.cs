using MatchingEngineService.Domain.Entities;

namespace MatchingEngineService.Domain
{
    /// <summary>
    /// In-memory price-time-priority order book for a single trading pair.
    /// Not thread-safe by design — callers must guarantee single-writer access (see OrderBookActor).
    /// </summary>
    public sealed class OrderBook
    {
        private readonly SortedDictionary<decimal, LinkedList<BookOrder>> _bids = new(Comparer<decimal>.Create((a, b) => b.CompareTo(a)));
        private readonly SortedDictionary<decimal, LinkedList<BookOrder>> _asks = new();
        private readonly Dictionary<Guid, (OrderSide Side, decimal Price)> _orderIndex = new();

        public string PairSymbol { get; }

        public OrderBook(string pairSymbol)
        {
            PairSymbol = pairSymbol;
        }

        public IReadOnlyList<Trade> Submit(BookOrder incoming)
        {
            var trades = new List<Trade>();
            var oppositeLevels = incoming.Side == OrderSide.Buy ? _asks : _bids;

            while (incoming.RemainingQuantity > 0 && oppositeLevels.Count > 0)
            {
                var bestLevel = oppositeLevels.First();
                var bestPrice = bestLevel.Key;

                if (incoming.Type == OrderType.Limit && !PriceCrosses(incoming.Side, incoming.Price, bestPrice))
                {
                    break;
                }

                var restingOrders = bestLevel.Value;
                var resting = restingOrders.First!.Value;

                var tradeQuantity = Math.Min(incoming.RemainingQuantity, resting.RemainingQuantity);
                incoming.RemainingQuantity -= tradeQuantity;
                resting.RemainingQuantity -= tradeQuantity;

                var buyOrderId = incoming.Side == OrderSide.Buy ? incoming.Id : resting.Id;
                var sellOrderId = incoming.Side == OrderSide.Sell ? incoming.Id : resting.Id;
                var buyRemaining = incoming.Side == OrderSide.Buy ? incoming.RemainingQuantity : resting.RemainingQuantity;
                var sellRemaining = incoming.Side == OrderSide.Sell ? incoming.RemainingQuantity : resting.RemainingQuantity;

                trades.Add(new Trade
                {
                    Id = Guid.NewGuid(),
                    PairSymbol = PairSymbol,
                    BuyOrderId = buyOrderId,
                    SellOrderId = sellOrderId,
                    Price = bestPrice,
                    Quantity = tradeQuantity,
                    ExecutedAt = DateTimeOffset.UtcNow,
                    BuyOrderRemainingQuantity = buyRemaining,
                    SellOrderRemainingQuantity = sellRemaining
                });

                if (resting.RemainingQuantity <= 0)
                {
                    restingOrders.RemoveFirst();
                    _orderIndex.Remove(resting.Id);
                    if (restingOrders.Count == 0)
                    {
                        oppositeLevels.Remove(bestPrice);
                    }
                }
            }

            if (incoming.Type == OrderType.Limit && incoming.RemainingQuantity > 0)
            {
                AddToBook(incoming);
            }

            return trades;
        }

        public bool Cancel(Guid orderId)
        {
            if (!_orderIndex.TryGetValue(orderId, out var location))
            {
                return false;
            }

            var levels = location.Side == OrderSide.Buy ? _bids : _asks;
            if (!levels.TryGetValue(location.Price, out var ordersAtLevel))
            {
                return false;
            }

            var node = ordersAtLevel.First;
            while (node is not null)
            {
                if (node.Value.Id == orderId)
                {
                    ordersAtLevel.Remove(node);
                    if (ordersAtLevel.Count == 0)
                    {
                        levels.Remove(location.Price);
                    }
                    _orderIndex.Remove(orderId);
                    return true;
                }
                node = node.Next;
            }

            return false;
        }

        private void AddToBook(BookOrder order)
        {
            var levels = order.Side == OrderSide.Buy ? _bids : _asks;
            if (!levels.TryGetValue(order.Price, out var ordersAtLevel))
            {
                ordersAtLevel = new LinkedList<BookOrder>();
                levels[order.Price] = ordersAtLevel;
            }

            ordersAtLevel.AddLast(order);
            _orderIndex[order.Id] = (order.Side, order.Price);
        }

        private static bool PriceCrosses(OrderSide incomingSide, decimal incomingPrice, decimal bestOppositePrice)
        {
            return incomingSide == OrderSide.Buy
                ? incomingPrice >= bestOppositePrice
                : incomingPrice <= bestOppositePrice;
        }
    }
}
