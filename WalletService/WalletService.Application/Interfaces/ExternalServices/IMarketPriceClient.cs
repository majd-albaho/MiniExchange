using WalletService.Application.Models;

namespace WalletService.Application.Interfaces.ExternalServices
{
    /// <summary>
    /// Reads current USDT prices from MarketDataService so wallet balances and transaction rows can
    /// be valued in USDT. Prices exist only for symbols MarketData is subscribed to.
    /// </summary>
    public interface IMarketPriceClient
    {
        /// <summary>Latest prices keyed by trading symbol (e.g. "BTCUSDT"). Empty if unavailable.</summary>
        Task<IReadOnlyDictionary<string, AssetPrice>> GetPricesAsync(CancellationToken cancellationToken = default);
    }
}
