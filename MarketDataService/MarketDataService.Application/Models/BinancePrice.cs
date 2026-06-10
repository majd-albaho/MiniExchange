namespace MarketDataService.Application.Models
{
    public sealed record BinancePrice(string Symbol, decimal LastPrice, decimal BidPrice, decimal AskPrice, DateTimeOffset EventTime);
}
