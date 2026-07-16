using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradingService.Api.Extensions;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/trades")]
    public sealed class TradesController : ControllerBase
    {
        private readonly ITradeHistoryService _tradeHistory;

        public TradesController(ITradeHistoryService tradeHistory)
        {
            _tradeHistory = tradeHistory;
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
        {
            var callerId = User.GetUserId();
            if (callerId is null)
                return Unauthorized();
            if (callerId != userId)
                return Forbid();

            try
            {
                var history = await _tradeHistory.GetByUserAsync(userId, page, pageSize, cancellationToken);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
