using System.Collections.Concurrent;

namespace MatchingEngineService.Application
{
    public sealed class OrderBookRegistry
    {
        private readonly ConcurrentDictionary<string, OrderBookActor> _actors = new();

        public OrderBookActor GetOrCreate(string pairSymbol)
        {
            return _actors.GetOrAdd(pairSymbol, static symbol => new OrderBookActor(symbol));
        }

        /// <summary>
        /// Returns the existing actor for a pair, or null. Used by read-only paths (order-book
        /// snapshots) so polling an untraded symbol doesn't spin up a background actor loop.
        /// </summary>
        public OrderBookActor? TryGet(string pairSymbol)
        {
            return _actors.TryGetValue(pairSymbol, out var actor) ? actor : null;
        }
    }
}
