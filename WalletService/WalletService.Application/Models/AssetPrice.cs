namespace WalletService.Application.Models
{
    /// <summary>Current USDT price and 24h change for one asset, sourced from MarketDataService.</summary>
    public sealed record AssetPrice(decimal Price, decimal Change24h);
}
