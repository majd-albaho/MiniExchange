namespace MiniExchange.Core.Models;

public sealed record OrderBookLevel(
    decimal Price,
    decimal Quantity
);