using MarketDataService.Application.Models;

namespace MarketDataService.Api.Services
{
    public interface IPriceCache
    {
        BinancePrice? Get(string symbol);

        void Set(BinancePrice price);
    }
}
