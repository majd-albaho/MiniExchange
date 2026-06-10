using Microsoft.AspNetCore.SignalR;

namespace MarketDataService.Infrastructure.Hubs
{
    public class MarketDataHub : Hub
    {
        public async Task Subscribe(string symbol)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, symbol.ToUpperInvariant());
        }

        public async Task Unsubscribe(string symbol)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol.ToUpperInvariant());
        }
    }
}
