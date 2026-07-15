using MatchingEngineService.Domain;
using MatchingEngineService.Domain.Entities;

namespace MatchingEngineService.Tests
{
    public class OrderBookSnapshotTests
    {
        private static BookOrder Limit(OrderSide side, decimal price, decimal quantity) => new()
        {
            Id = Guid.NewGuid(),
            PairSymbol = "BTCUSDT",
            Side = side,
            Type = OrderType.Limit,
            Price = price,
            RemainingQuantity = quantity,
            CreatedDate = DateTimeOffset.UtcNow
        };

        [Fact]
        public void GetSnapshot_OrdersLevelsBestFirst_AndAggregatesQuantityPerPrice()
        {
            var book = new OrderBook("BTCUSDT");
            book.Submit(Limit(OrderSide.Buy, 100m, 1m));
            book.Submit(Limit(OrderSide.Buy, 100m, 2m)); // same level as above → aggregated
            book.Submit(Limit(OrderSide.Buy, 99m, 5m));
            book.Submit(Limit(OrderSide.Sell, 101m, 3m));
            book.Submit(Limit(OrderSide.Sell, 102m, 4m));

            var snapshot = book.GetSnapshot(10);

            Assert.Equal(100m, snapshot.Bids[0].Price);   // best (highest) bid first
            Assert.Equal(3m, snapshot.Bids[0].Quantity);  // 1 + 2 aggregated
            Assert.Equal(99m, snapshot.Bids[1].Price);

            Assert.Equal(101m, snapshot.Asks[0].Price);   // best (lowest) ask first
            Assert.Equal(3m, snapshot.Asks[0].Quantity);
            Assert.Equal(102m, snapshot.Asks[1].Price);
        }

        [Fact]
        public void GetSnapshot_RespectsDepthLimit()
        {
            var book = new OrderBook("BTCUSDT");
            for (var i = 1; i <= 5; i++)
            {
                book.Submit(Limit(OrderSide.Buy, 100m - i, 1m));
            }

            var snapshot = book.GetSnapshot(2);

            Assert.Equal(2, snapshot.Bids.Count);
            Assert.Equal(99m, snapshot.Bids[0].Price);
            Assert.Equal(98m, snapshot.Bids[1].Price);
        }

        [Fact]
        public void GetSnapshot_ExcludesFullyMatchedRestingQuantity()
        {
            var book = new OrderBook("BTCUSDT");
            book.Submit(Limit(OrderSide.Sell, 100m, 2m));
            book.Submit(Limit(OrderSide.Buy, 100m, 3m)); // takes the 2, leaves 1 resting on the bid

            var snapshot = book.GetSnapshot(10);

            Assert.Empty(snapshot.Asks);
            Assert.Single(snapshot.Bids);
            Assert.Equal(100m, snapshot.Bids[0].Price);
            Assert.Equal(1m, snapshot.Bids[0].Quantity);
        }
    }
}
