using MarketDataService.Api.Services;
using MarketDataService.Application.Models;
using System.Globalization;

namespace MarketDataService.Tests
{
    public class BinancePriceTests
    {
        [Fact]
        public void PriceCacheReturnsCachedPriceForCaseInsensitiveSymbol()
        {
            IPriceCache cache = new InMemoryPriceCache();
            var price = new BinancePrice(
                "BTCUSDT",
                100000.12m,
                99999.99m,
                100000.13m,
                DateTimeOffset.FromUnixTimeMilliseconds(1_725_000_000_000));

            cache.Set(price);

            Assert.Equal(price, cache.Get("btcusdt"));
            Assert.Equal(price, cache.Get(" BTCUSDT "));
            Assert.Null(cache.Get("BTC-USDT"));
        }

        [Fact]
        public void BinanceTickerParsesPricesUsingInvariantCulture()
        {
            var previousCulture = CultureInfo.CurrentCulture;

            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

                var ticker = new BinanceTicker
                {
                    c = "100000.12",
                    b = "99999.99",
                    a = "100000.13"
                };

                Assert.Equal(100000.12m, ticker.LastPrice);
                Assert.Equal(99999.99m, ticker.BidPrice);
                Assert.Equal(100000.13m, ticker.AskPrice);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
        }
    }
}
