namespace MarketDataService.Application.Interfaces.Services
{
    public interface ISubscriptionService
    {
        Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
