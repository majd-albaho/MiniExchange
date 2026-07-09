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
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(IWalletFundService walletFundService, IWalletTransactionService walletTransactionService, ILogger<TransactionsController> logger)
        {
            _walletFundService = walletFundService;
            _walletTransactionService = walletTransactionService;
            _logger = logger;
        }

        [HttpPost("AddressTransaction")]
        public async Task<IActionResult> AddressTransaction([FromBody] AlchemyWebhookPayload payload, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Alchemy webhook received. WebhookId: {payload.WebhookId}, EventCount: {payload.Event?.Activity?.Count ?? 0}");

                foreach (var activity in payload.Event?.Activity ?? [])
                {
                    _logger.LogInformation(
                        $"Transfer detected. Hash: {activity.Hash}, From: {activity.FromAddress}, To: {activity.ToAddress}," +
                        $" Asset: {activity.Asset}, Value: {activity.Value}, Category: {activity.Category}");

                    // TODO:
                    // 1. Check if ToAddress belongs to registered wallet
                    // 2. Check idempotency by Hash + UniqueId
                    // 3. Save BlockchainTransaction
                    // 4. Publish DepositDetectedEvent or credit WalletService
                }


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

        /// <summary>
        /// Dev/testing only: credits a balance with no real deposit behind it. There is no real
        /// fiat/BTC deposit flow in this sandbox today (only ETH via Nethereum), so this is how
        /// test accounts get funded to place orders.
        /// </summary>
        [HttpPost("Credit")]
        public async Task<IActionResult> Credit(CreditFundRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _walletFundService.CreditFund(request.UserId, request.AssetName, request.Amount, cancellationToken);
                return Ok();
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
