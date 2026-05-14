using MiniExchange.Core.Models;

namespace MiniExchange.Core.Engine;

public sealed class OrderBook
{
    private readonly List<Order> _buyOrders = [];
    private readonly List<Order> _sellOrders = [];

    public IReadOnlyList<Order> BuyOrders => _buyOrders;
    public IReadOnlyList<Order> SellOrders => _sellOrders;

    public List<Trade> AddOrder(Order incomingOrder)
    {
        return incomingOrder.Side switch
        {
            OrderSide.Buy => MatchBuyOrder(incomingOrder),
            OrderSide.Sell => MatchSellOrder(incomingOrder),
            _ => throw new InvalidOperationException("Unknown order side.")
        };
    }

    private List<Trade> MatchBuyOrder(Order buyOrder)
    {
        List<Trade> trades = [];

        while (buyOrder.RemainingQuantity > 0)
        {
            var bestSell = _sellOrders
                .Where(o => o.Status is OrderStatus.Open or OrderStatus.PartiallyFilled)
                .OrderBy(o => o.Price)
                .ThenBy(o => o.CreatedAt)
                .FirstOrDefault();

            if (bestSell is null)
                break;

            if (buyOrder.Type == OrderType.Limit && buyOrder.Price < bestSell.Price)
                break;

            var quantity = Math.Min(buyOrder.RemainingQuantity, bestSell.RemainingQuantity);

            buyOrder.Fill(quantity);
            bestSell.Fill(quantity);

            trades.Add(new Trade
            {
                Symbol = buyOrder.Symbol,
                BuyOrderId = buyOrder.Id,
                SellOrderId = bestSell.Id,
                Price = bestSell.Price,
                Quantity = quantity
            });

            _sellOrders.RemoveAll(o => o.Status == OrderStatus.Filled);
        }

        if (buyOrder.RemainingQuantity > 0)
        {
            if (buyOrder.Type == OrderType.Limit)
            {
                _buyOrders.Add(buyOrder);
            }
            else
            {
                buyOrder.Cancel();
            }
        }

        SortBooks();

        return trades;
    }

    private List<Trade> MatchSellOrder(Order sellOrder)
    {
        List<Trade> trades = [];

        while (sellOrder.RemainingQuantity > 0)
        {
            var bestBuy = _buyOrders
                .Where(o => o.Status is OrderStatus.Open or OrderStatus.PartiallyFilled)
                .OrderByDescending(o => o.Price)
                .ThenBy(o => o.CreatedAt)
                .FirstOrDefault();

            if (bestBuy is null)
                break;

            if (sellOrder.Type == OrderType.Limit && sellOrder.Price > bestBuy.Price)
                break;

            var quantity = Math.Min(sellOrder.RemainingQuantity, bestBuy.RemainingQuantity);

            sellOrder.Fill(quantity);
            bestBuy.Fill(quantity);

            trades.Add(new Trade
            {
                Symbol = sellOrder.Symbol,
                BuyOrderId = bestBuy.Id,
                SellOrderId = sellOrder.Id,
                Price = bestBuy.Price,
                Quantity = quantity
            });

            _buyOrders.RemoveAll(o => o.Status == OrderStatus.Filled);
        }

        if (sellOrder.RemainingQuantity > 0)
        {
            if (sellOrder.Type == OrderType.Limit)
            {
                _sellOrders.Add(sellOrder);
            }
            else
            {
                sellOrder.Cancel();
            }
        }

        SortBooks();

        return trades;
    }

    private void SortBooks()
    {
        _buyOrders.Sort((a, b) =>
        {
            var priceComparison = b.Price.CompareTo(a.Price);
            return priceComparison != 0
                ? priceComparison
                : a.CreatedAt.CompareTo(b.CreatedAt);
        });

        _sellOrders.Sort((a, b) =>
        {
            var priceComparison = a.Price.CompareTo(b.Price);
            return priceComparison != 0
                ? priceComparison
                : a.CreatedAt.CompareTo(b.CreatedAt);
        });
    }

    public bool CancelOrder(Guid orderId)
    {
        var order = _buyOrders.FirstOrDefault(o => o.Id == orderId)
            ?? _sellOrders.FirstOrDefault(o => o.Id == orderId);

        if (order is null)
            return false;

        order.Cancel();

        _buyOrders.RemoveAll(o => o.Id == orderId);
        _sellOrders.RemoveAll(o => o.Id == orderId);

        return true;
    }

    public Order? GetBestBid()
    {
        return _buyOrders
            .Where(o => o.Status is OrderStatus.Open or OrderStatus.PartiallyFilled)
            .OrderByDescending(o => o.Price)
            .ThenBy(o => o.CreatedAt)
            .FirstOrDefault();
    }

    public Order? GetBestAsk()
    {
        return _sellOrders
            .Where(o => o.Status is OrderStatus.Open or OrderStatus.PartiallyFilled)
            .OrderBy(o => o.Price)
            .ThenBy(o => o.CreatedAt)
            .FirstOrDefault();
    }

    public decimal? GetSpread()
    {
        var bestBid = GetBestBid();
        var bestAsk = GetBestAsk();

        if (bestBid is null || bestAsk is null)
            return null;

        return bestAsk.Price - bestBid.Price;
    }

    public IReadOnlyList<OrderBookLevel> GetBidDepth(int limit = 10)
    {
        return _buyOrders
            .Where(o => o.Status is OrderStatus.Open or OrderStatus.PartiallyFilled)
            .GroupBy(o => o.Price)
            .Select(g => new OrderBookLevel(
                Price: g.Key,
                Quantity: g.Sum(o => o.RemainingQuantity)
            ))
            .OrderByDescending(level => level.Price)
            .Take(limit)
            .ToList();
    }

    public IReadOnlyList<OrderBookLevel> GetAskDepth(int limit = 10)
    {
        return _sellOrders
            .Where(o => o.Status is OrderStatus.Open or OrderStatus.PartiallyFilled)
            .GroupBy(o => o.Price)
            .Select(g => new OrderBookLevel(
                Price: g.Key,
                Quantity: g.Sum(o => o.RemainingQuantity)
            ))
            .OrderBy(level => level.Price)
            .Take(limit)
            .ToList();
    }
}