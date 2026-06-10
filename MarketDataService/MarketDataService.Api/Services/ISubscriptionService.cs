namespace MarketDataService.Api.Services
{
    public interface ISubscriptionService
    {
        Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
