using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders)
        {
            _orders = orders;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orders.GetByIdAsync(id, cancellationToken);
                if (order == null)
                    return NotFound();

                return Ok(order);
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _orders.CreateAsync(request, cancellationToken);
                return Ok(order);
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string? deletedBy, CancellationToken cancellationToken)
        {
            try
            {
                var deleted = await _orders.DeleteAsync(id, deletedBy ?? string.Empty, cancellationToken);
                return deleted ? NoContent() : NotFound();
            }
            catch (AuthenticationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
