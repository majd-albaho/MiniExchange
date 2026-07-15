namespace MarketDataService.Application.Models
{
    /// <summary>
    /// 24h rolling-window stats for a symbol, sourced from Binance's ticker stream.
    /// Distinct from <see cref="CryptoCurrencyPrice"/> (last/bid/ask only) used on the price hot path.
    /// </summary>
    public sealed record MarketTicker(
        string Symbol,
        decimal LastPrice,
        decimal PriceChangePercent,
        decimal HighPrice,
        decimal LowPrice,
        decimal BaseVolume,
        decimal QuoteVolume,
        DateTimeOffset EventTime);
}
