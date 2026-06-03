using Microsoft.AspNetCore.Mvc;
using WalletService.Application.Interfaces.Services;
using WalletService.Application.Models;

namespace WalletService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly IWalletFundService _walletFundService;
        private readonly IWalletTransactionService _walletTransactionService;

        public TransactionsController(IWalletFundService walletFundService, IWalletTransactionService walletTransactionService)
        {
            _walletFundService = walletFundService;
            _walletTransactionService = walletTransactionService;
        }

        [HttpPost("LockFund")]
        public async Task<IActionResult> LockFund(FundLockRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _walletFundService.LockFund(request.UserId, request.AssetId, request.Amount, cancellationToken);
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
                await _walletFundService.UnlockFund(request.UserId, request.AssetId, request.Amount, cancellationToken);
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
                var transactionDetails = await _walletTransactionService.GetTransactionDetails(transactionId, cancellationToken);
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
