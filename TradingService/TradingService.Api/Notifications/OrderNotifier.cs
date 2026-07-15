using Microsoft.AspNetCore.SignalR;
using TradingService.Api.Hubs;
using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Api.Notifications
{
    public sealed class OrderNotifier : IOrderNotifier
    {
        private readonly IHubContext<OrderHub> _hub;

        public OrderNotifier(IHubContext<OrderHub> hub)
        {
            _hub = hub;
        }

        public Task OrderUpdatedAsync(Guid userId, OrderUpdateNotification notification, CancellationToken cancellationToken = default)
        {
            return _hub.Clients.Group(OrderHub.UserGroup(userId)).SendAsync("OrderUpdated", notification, cancellationToken);
        }
    }
}
