namespace MiniExchange.Core.Models;

public sealed class Trade
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Symbol { get; init; } = "BTC-USDT";

    public Guid BuyOrderId { get; init; }

    public Guid SellOrderId { get; init; }

    public decimal Price { get; init; }

    public decimal Quantity { get; init; }

    public DateTimeOffset ExecutedAt { get; init; } = DateTimeOffset.UtcNow;
}