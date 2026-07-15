using TradingService.Application.Dto;
using TradingService.Application.Interfaces.Clients;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Application.Services
{
    /// <summary>
    /// Public-facing view of the matching engine's in-memory book. Reshapes the engine's
    /// per-level quantities into the price/amount/total rows the frontend renders.
    /// </summary>
    public sealed class OrderBookService : IOrderBookService
    {
        private const int DefaultDepth = 20;
        private const int MaxDepth = 100;

        private readonly IMatchingEngineClient _matchingEngine;

        public OrderBookService(IMatchingEngineClient matchingEngine)
        {
            _matchingEngine = matchingEngine;
        }

        public async Task<OrderBookResponse> GetOrderBookAsync(string pairSymbol, int depth, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pairSymbol))
            {
                throw new ArgumentException("Pair symbol is required.", nameof(pairSymbol));
            }

            var normalizedSymbol = pairSymbol.Trim().ToUpperInvariant();
            var clampedDepth = depth <= 0 ? DefaultDepth : Math.Min(depth, MaxDepth);

            var snapshot = await _matchingEngine.GetOrderBookAsync(normalizedSymbol, clampedDepth, cancellationToken);

            return new OrderBookResponse
            {
                Pair = normalizedSymbol,
                Asks = snapshot.Asks.Select(ToEntry).ToList(),
                Bids = snapshot.Bids.Select(ToEntry).ToList(),
                LastUpdateTime = DateTimeOffset.UtcNow
            };
        }

        private static OrderBookEntryResponse ToEntry(OrderBookLevelDto level)
        {
            return new OrderBookEntryResponse
            {
                Price = level.Price,
                Amount = level.Quantity,
                Total = level.Price * level.Quantity
            };
        }
    }
}
