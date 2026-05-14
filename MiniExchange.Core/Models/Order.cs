namespace MiniExchange.Core.Models;

public enum OrderSide
{
    Buy,
    Sell
}

public enum OrderType
{
    Limit,
    Market
}

public enum OrderStatus
{
    Open,
    PartiallyFilled,
    Filled,
    Cancelled
}

public sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Symbol { get; init; } = "BTC-USDT";

    public OrderSide Side { get; init; }

    public OrderType Type { get; init; }

    public decimal Price { get; init; }

    public decimal Quantity { get; init; }

    public decimal FilledQuantity { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.Open;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public decimal RemainingQuantity => Quantity - FilledQuantity;

    public void Fill(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Fill quantity must be positive.");

        if (quantity > RemainingQuantity)
            throw new InvalidOperationException("Cannot fill more than remaining quantity.");

        FilledQuantity += quantity;

        Status = RemainingQuantity == 0
            ? OrderStatus.Filled
            : OrderStatus.PartiallyFilled;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Filled)
            throw new InvalidOperationException("Cannot cancel a filled order.");

        Status = OrderStatus.Cancelled;
    }
}