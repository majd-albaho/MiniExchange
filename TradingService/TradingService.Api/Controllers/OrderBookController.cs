using Microsoft.AspNetCore.Mvc;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Api.Controllers
{
    [ApiController]
    [Route("api/orderbook")]
    public sealed class OrderBookController : ControllerBase
    {
        private readonly IOrderBookService _orderBook;

        public OrderBookController(IOrderBookService orderBook)
        {
            _orderBook = orderBook;
        }

        [HttpGet("{symbol}")]
        public async Task<IActionResult> Get(string symbol, [FromQuery] int depth = 20, CancellationToken cancellationToken = default)
        {
            try
            {
                var book = await _orderBook.GetOrderBookAsync(symbol, depth, cancellationToken);
                return Ok(book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
