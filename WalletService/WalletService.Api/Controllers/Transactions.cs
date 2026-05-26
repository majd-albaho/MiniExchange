using Microsoft.AspNetCore.Mvc;
using WalletService.Application.Interfaces.Services;

namespace WalletService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Transactions : ControllerBase
    {
        private readonly IUserWalletService _userWalletService;

        public Transactions(IUserWalletService userWalletService) {
            _userWalletService = userWalletService;
        }


        [HttpPost("{transactionId}")]
        public async Task<IActionResult> Get(string transactionId, CancellationToken cancellationToken) {
            try {
                var transactionDetails = await _userWalletService.GetTransactionDetails(transactionId);
                return Ok(new { TransactionDetails = transactionDetails });
            } catch (UnauthorizedAccessException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
