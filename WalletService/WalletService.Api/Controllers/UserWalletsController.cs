using Microsoft.AspNetCore.Mvc;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;

namespace WalletService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserWalletsController : ControllerBase
    {
        private readonly IUserWalletService _userWalletService;

        public UserWalletsController(IUserWalletService userWalletService) {
            _userWalletService = userWalletService;
        }

        [HttpPost("Balance/{userId}")]
        public async Task<IActionResult> GetUserWalletBalance(Guid userId, CancellationToken cancellationToken) {
            try {
                var balance = await _userWalletService.CheckBalance(userId);
                return Ok(new { Balance = balance });
            } catch (UnauthorizedAccessException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send(SendRequest sendRequest, CancellationToken cancellationToken) {
            try {
                var transaction = await _userWalletService.SendEtherium(sendRequest.UserId, sendRequest.RecipientAddress, sendRequest.Amount);
                return Ok(new { transaction });
            } catch (UnauthorizedAccessException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
