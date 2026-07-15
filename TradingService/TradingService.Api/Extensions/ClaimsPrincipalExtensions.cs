using System.Security.Claims;

namespace TradingService.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>The authenticated user's id, taken from the JWT subject claim.</summary>
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
