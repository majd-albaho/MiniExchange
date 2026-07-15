using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletService.Api.Extensions;
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
        private readonly IWalletDepositService _walletDepositService;
        private readonly IWalletTransactionHistoryService _walletTransactionHistoryService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            IWalletFundService walletFundService,
            IWalletTransactionService walletTransactionService,
            IWalletDepositService walletDepositService,
            IWalletTransactionHistoryService walletTransactionHistoryService,
            ILogger<TransactionsController> logger)
        {
            _walletFundService = walletFundService;
            _walletTransactionService = walletTransactionService;
            _walletDepositService = walletDepositService;
            _walletTransactionHistoryService = walletTransactionHistoryService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserTransactions(Guid userId, [FromQuery] WalletTransactionHistoryQuery query, CancellationToken cancellationToken)
        {
            var callerId = User.GetUserId();
            if (callerId is null)
                return Unauthorized();
            if (callerId != userId)
                return Forbid();

            try
            {
                var history = await _walletTransactionHistoryService.GetHistoryAsync(userId, query, cancellationToken);
                return Ok(history);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

                    var credited = await _walletDepositService.ProcessDepositAsync(
                        activity.ToAddress, activity.Asset, activity.Value, activity.Hash, cancellationToken);

                    if (credited)
                        _logger.LogInformation("Deposit credited for tx {TxHash} to {ToAddress}", activity.Hash, activity.ToAddress);
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

        /// <summary>
        /// Adds a ledger-only demo token for testing (e.g. a fake BTC/USDT balance). Demo tokens
        /// are flagged so they can be traded in the sandbox but can never be withdrawn on-chain.
        /// </summary>
        [Authorize]
        [HttpPost("AddDemoToken")]
        public async Task<IActionResult> AddDemoToken(AddDemoTokenRequest request, CancellationToken cancellationToken)
        {
            var callerId = User.GetUserId();
            if (callerId is null)
                return Unauthorized();

            try
            {
                // Demo tokens are always credited to the authenticated user's own wallet.
                await _walletFundService.AddDemoTokenAsync(callerId.Value, request.AssetName, request.Amount, cancellationToken);
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
