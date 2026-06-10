using MarketDataService.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketDataService.Api.Controllers
{
    [ApiController]
    [Route("api/binance")]
    public sealed class BinancePriceController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPriceCache _priceCache;

        public BinancePriceController(ISubscriptionService subscriptionService, IPriceCache priceCache)
        {
            _subscriptionService = subscriptionService;
            _priceCache = priceCache;
        }

        [HttpPost("subscribe/{symbol}")]
        public async Task<IActionResult> Subscribe(string symbol, CancellationToken cancellationToken)
        {
            if (!BinanceSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return BadRequest(new { Message = "Symbol must contain 1 to 32 ASCII letters or digits." });
            }

            await _subscriptionService.SubscribeAsync(normalizedSymbol, cancellationToken);

            return Ok(new
            {
                Symbol = normalizedSymbol,
                Message = "Subscription started"
            });
        }

        [HttpGet("price/{symbol}")]
        public IActionResult GetLatestPrice(string symbol)
        {
            if (!BinanceSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return BadRequest(new { Message = "Symbol must contain 1 to 32 ASCII letters or digits." });
            }

            var price = _priceCache.Get(normalizedSymbol);

            if (price == null)
            {
                return NotFound();
            }

            return Ok(price);
        }
    }
}
