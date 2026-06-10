using MarketDataService.Application.Models;

namespace MarketDataService.Application.Interfaces.Services
{
    public interface IPriceCache
    {
        CryptoCurrencyPrice? Get(string symbol);

        void Set(CryptoCurrencyPrice price);
    }
}
