using Microsoft.AspNetCore.Mvc;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;

namespace WalletService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly IUserWalletService _userWalletService;

        public TransactionsController(IUserWalletService userWalletService)
        {
            _userWalletService = userWalletService;
        }

        [HttpPost("LockFund")]
        public async Task<IActionResult> LockFund(FundLockRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _userWalletService.LockFund(request.UserId, request.AssetId, request.Amount, cancellationToken);
                return Ok();
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

        [HttpPost("UnlockFund")]
        public async Task<IActionResult> UnlockFund(FundLockRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _userWalletService.UnlockFund(request.UserId, request.AssetId, request.Amount, cancellationToken);
                return Ok();
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

        [HttpPost("{transactionId}")]
        public async Task<IActionResult> Get(string transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var transactionDetails = await _userWalletService.GetTransactionDetails(transactionId);
                return Ok(new { TransactionDetails = transactionDetails });
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
