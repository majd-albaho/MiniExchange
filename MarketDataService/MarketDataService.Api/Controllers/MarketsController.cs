using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Domain.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace MarketDataService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class MarketsController : ControllerBase
    {
        private static readonly HashSet<string> AllowedIntervals = new(StringComparer.Ordinal)
        {
            "1m", "3m", "5m", "15m", "30m", "1h", "2h", "4h", "6h", "8h", "12h", "1d", "3d", "1w", "1M"
        };

        private readonly ISubscriptionService _subscriptionService;
        private readonly IPriceCache _priceCache;
        private readonly ICandleService _candleService;
        private readonly ITickerCache _tickerCache;

        public MarketsController(ISubscriptionService subscriptionService, IPriceCache priceCache, ICandleService candleService, ITickerCache tickerCache)
        {
            _subscriptionService = subscriptionService;
            _priceCache = priceCache;
            _candleService = candleService;
            _tickerCache = tickerCache;
        }

        [HttpGet("tickers")]
        public IActionResult GetTickers()
        {
            return Ok(_tickerCache.GetAll());
        }

        [HttpGet("ticker/{symbol}")]
        public IActionResult GetTicker(string symbol)
        {
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return BadRequest(new { Message = "Symbol must contain 1 to 32 ASCII letters or digits." });
            }

            var ticker = _tickerCache.Get(normalizedSymbol);
            return ticker == null ? NotFound() : Ok(ticker);
        }

        [HttpPost("subscribe/{symbol}")]
        public async Task<IActionResult> Subscribe(string symbol, CancellationToken cancellationToken)
        {
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
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
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
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

        [HttpGet("candles/{symbol}")]
        public async Task<IActionResult> GetCandles(string symbol, [FromQuery] string interval = "1h", [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
        {
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                return BadRequest(new { Message = "Symbol must contain 1 to 32 ASCII letters or digits." });
            }

            if (!AllowedIntervals.Contains(interval))
            {
                return BadRequest(new { Message = "Unsupported interval." });
            }

            var clampedLimit = Math.Clamp(limit, 1, 1000);

            var candles = await _candleService.GetCandlesAsync(normalizedSymbol, interval, clampedLimit, cancellationToken);
            return Ok(candles);
        }
    }
}
