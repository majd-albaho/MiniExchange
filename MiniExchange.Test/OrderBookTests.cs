using MiniExchange.Core.Engine;
using MiniExchange.Core.Models;

namespace MiniExchange.Tests;

public sealed class OrderBookTests
{
    [Test]
    public void BuyOrder_ShouldMatchExistingSellOrder()
    {
        var book = new OrderBook();

        var sell = new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 100m,
            Quantity = 1m
        };

        book.AddOrder(sell);

        var buy = new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 101m,
            Quantity = 1m
        };

        var trades = book.AddOrder(buy);

        Assert.That(trades, Has.Count.EqualTo(1));
        Assert.That(trades[0].Price, Is.EqualTo(100m));
        Assert.That(trades[0].Quantity, Is.EqualTo(1m));

        Assert.That(book.SellOrders, Is.Empty);
        Assert.That(book.BuyOrders, Is.Empty);
    }

    [Test]
    public void CancelOrder_ShouldRemoveOrderFromBook()
    {
        var book = new OrderBook();

        var buy = new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 100m,
            Quantity = 1m
        };

        book.AddOrder(buy);

        var cancelled = book.CancelOrder(buy.Id);

        Assert.That(cancelled, Is.True);
        Assert.That(book.BuyOrders, Is.Empty);
    }

    [Test]
    public void GetBestBid_ShouldReturnHighestBuyPrice()
    {
        var book = new OrderBook();

        book.AddOrder(new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 99m,
            Quantity = 1m
        });

        book.AddOrder(new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 101m,
            Quantity = 1m
        });

        var bestBid = book.GetBestBid();

        Assert.That(bestBid, Is.Not.Null);
        Assert.That(bestBid!.Price, Is.EqualTo(101m));
    }

    [Test]
    public void GetBestAsk_ShouldReturnLowestSellPrice()
    {
        var book = new OrderBook();

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 105m,
            Quantity = 1m
        });

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 102m,
            Quantity = 1m
        });

        var bestAsk = book.GetBestAsk();

        Assert.That(bestAsk, Is.Not.Null);
        Assert.That(bestAsk!.Price, Is.EqualTo(102m));
    }

    [Test]
    public void GetSpread_ShouldReturnDifferenceBetweenBestAskAndBestBid()
    {
        var book = new OrderBook();

        book.AddOrder(new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 100m,
            Quantity = 1m
        });

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 103m,
            Quantity = 1m
        });

        var spread = book.GetSpread();

        Assert.That(spread, Is.EqualTo(3m));
    }

    [Test]
    public void GetBidDepth_ShouldAggregateBuyOrdersByPrice()
    {
        var book = new OrderBook();

        book.AddOrder(new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 100m,
            Quantity = 1m
        });

        book.AddOrder(new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            Price = 100m,
            Quantity = 2m
        });

        var depth = book.GetBidDepth();

        Assert.That(depth, Has.Count.EqualTo(1));
        Assert.That(depth[0].Price, Is.EqualTo(100m));
        Assert.That(depth[0].Quantity, Is.EqualTo(3m));
    }

    [Test]
    public void GetAskDepth_ShouldAggregateSellOrdersByPrice()
    {
        var book = new OrderBook();

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 105m,
            Quantity = 1m
        });

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 105m,
            Quantity = 4m
        });

        var depth = book.GetAskDepth();

        Assert.That(depth, Has.Count.EqualTo(1));
        Assert.That(depth[0].Price, Is.EqualTo(105m));
        Assert.That(depth[0].Quantity, Is.EqualTo(5m));
    }

    [Test]
    public void MarketBuy_ShouldConsumeLowestSellOrders()
    {
        var book = new OrderBook();

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 100m,
            Quantity = 1m
        });

        book.AddOrder(new Order
        {
            Side = OrderSide.Sell,
            Type = OrderType.Limit,
            Price = 101m,
            Quantity = 2m
        });

        var marketBuy = new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Market,
            Quantity = 3m
        };

        var trades = book.AddOrder(marketBuy);

        Assert.That(trades, Has.Count.EqualTo(2));

        Assert.That(trades[0].Price, Is.EqualTo(100m));
        Assert.That(trades[1].Price, Is.EqualTo(101m));

        Assert.That(book.SellOrders, Is.Empty);
    }

    [Test]
    public void UnfilledMarketOrder_ShouldNotRemainInBook()
    {
        var book = new OrderBook();

        var marketBuy = new Order
        {
            Side = OrderSide.Buy,
            Type = OrderType.Market,
            Quantity = 10m
        };

        book.AddOrder(marketBuy);

        Assert.That(book.BuyOrders, Is.Empty);
    }
}