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
            try
            {
                var transaction = await _walletTransactionService.Send(sendRequest.UserId, sendRequest.AssetSymbol, sendRequest.RecipientAddress, sendRequest.Amount, cancellationToken);
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
    }
}
