namespace TradingService.Application.Interfaces.Clients
{
    public interface IMarketDataClient
    {
        /// <summary>Returns null if no live price has been cached yet for the symbol.</summary>
        Task<decimal?> GetLatestPriceAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
