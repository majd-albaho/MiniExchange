using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletService.Api.Extensions;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;

namespace WalletService.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserWalletsController : ControllerBase
    {
        private readonly IUserWalletService _userWalletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletOverviewService _walletOverviewService;

        public UserWalletsController(
            IUserWalletService userWalletService,
            IWalletTransactionService walletTransactionService,
            IWalletOverviewService walletOverviewService)
        {
            _userWalletService = userWalletService;
            _walletTransactionService = walletTransactionService;
            _walletOverviewService = walletOverviewService;
        }

        [HttpGet("{userId}/overview")]
        public async Task<IActionResult> GetOverview(Guid userId, CancellationToken cancellationToken)
        {
            var forbidden = EnsureCaller(userId, out var error);
            if (forbidden != null)
                return forbidden;

            try
            {
                var overview = await _walletOverviewService.GetOverviewAsync(userId, cancellationToken);
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{userId}/receive")]
        public async Task<IActionResult> GetReceiveInfo(Guid userId, [FromQuery] string symbol, [FromQuery] string network, CancellationToken cancellationToken)
        {
            var forbidden = EnsureCaller(userId, out _);
            if (forbidden != null)
                return forbidden;

            try
            {
                var info = await _walletOverviewService.GetReceiveInfoAsync(userId, symbol, network, cancellationToken);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Balance/{userId}")]
        public async Task<IActionResult> GetUserWalletBalance(Guid userId, CancellationToken cancellationToken)
        {
            var forbidden = EnsureCaller(userId, out _);
            if (forbidden != null)
                return forbidden;

            try
            {
                var balance = await _userWalletService.CheckEthereumBalance(userId, cancellationToken);
                return Ok(new { Balance = balance });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("Send")]
        public async Task<IActionResult> Send(SendRequest sendRequest, CancellationToken cancellationToken)
        {
            var callerId = User.GetUserId();
            if (callerId is null)
                return Unauthorized();

            try
            {
                // Always send from the authenticated user's wallet, ignoring any id in the body.
                var transaction = await _walletTransactionService.Send(callerId.Value, sendRequest.AssetSymbol, sendRequest.RecipientAddress, sendRequest.Amount, cancellationToken);
                return Ok(new { txId = transaction });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Returns a 401/403 result if the caller is not the owner of <paramref name="userId"/>, else null.</summary>
        private IActionResult? EnsureCaller(Guid userId, out string? error)
        {
            error = null;
            var callerId = User.GetUserId();
            if (callerId is null)
                return Unauthorized();
            if (callerId != userId)
                return Forbid();
            return null;
        }
    }
}
