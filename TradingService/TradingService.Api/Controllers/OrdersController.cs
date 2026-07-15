using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using TradingService.Api.Extensions;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Api.Controllers
{
    [ApiController]
    [Authorize]
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
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            try
            {
                var order = await _orders.GetByIdAsync(id, cancellationToken);
                if (order == null || order.UserId != userId)
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

        [HttpGet("user/{userId:guid}/open")]
        public async Task<IActionResult> GetOpenByUser(Guid userId, CancellationToken cancellationToken)
        {
            var callerId = User.GetUserId();
            if (callerId is null)
                return Unauthorized();
            if (userId != callerId)
                return Forbid();

            try
            {
                var orders = await _orders.GetOpenByUserAsync(userId, cancellationToken);
                return Ok(orders);
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
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            // The order is always placed as the authenticated user, regardless of any id in the body.
            request.UserId = userId.Value;

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
            var userId = User.GetUserId();
            if (userId is null)
                return Unauthorized();

            try
            {
                var order = await _orders.GetByIdAsync(id, cancellationToken);
                if (order == null || order.UserId != userId)
                    return NotFound();

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
