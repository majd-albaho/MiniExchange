namespace MarketDataService.Application.Models
{
    public sealed record CryptoCurrencyPrice(string Symbol, decimal LastPrice, decimal BidPrice, decimal AskPrice, DateTimeOffset EventTime);
}
