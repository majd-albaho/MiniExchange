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
    }
}
