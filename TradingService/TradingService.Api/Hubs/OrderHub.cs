using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TradingService.Api.Extensions;

namespace TradingService.Api.Hubs
{
    /// <summary>
    /// Per-user push channel for order/balance updates. Each connection is placed in a group keyed
    /// by the authenticated user's id, so a fill is only ever delivered to that user's own clients.
    /// </summary>
    [Authorize]
    public sealed class OrderHub : Hub
    {
        public static string UserGroup(Guid userId) => $"user-{userId}";

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.GetUserId();
            if (userId is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId.Value));
            }

            await base.OnConnectedAsync();
        }
    }
}
