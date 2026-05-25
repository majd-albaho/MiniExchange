using Microsoft.AspNetCore.Mvc;
using WalletService.Application.Interfaces.Services;

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
                var userWallet = await _userWalletService.GetUserWallet(userId);
                if (userWallet == null)
                    return NotFound();

                var balance = await _userWalletService.CheckBalance(userId);
                return Ok(new { Balance = balance.Value.ToString() });
            } catch (UnauthorizedAccessException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
