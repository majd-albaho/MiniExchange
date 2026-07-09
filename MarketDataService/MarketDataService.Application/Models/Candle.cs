namespace MarketDataService.Application.Models
{
    public sealed record Candle(long Time, decimal Open, decimal High, decimal Low, decimal Close, decimal Volume);
}
