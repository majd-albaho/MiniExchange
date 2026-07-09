using System.Threading.Channels;
using MatchingEngineService.Domain;
using MatchingEngineService.Domain.Entities;

namespace MatchingEngineService.Application
{
    /// <summary>
    /// Single-writer wrapper around one pair's in-memory OrderBook. All mutations are funneled
    /// through one background loop reading from an unbounded channel, so the book itself never
    /// needs locking on the matching hot path.
    /// </summary>
    public sealed class OrderBookActor
    {
        private readonly OrderBook _book;
        private readonly Channel<Action> _commands = Channel.CreateUnbounded<Action>();

        public OrderBookActor(string pairSymbol)
        {
            _book = new OrderBook(pairSymbol);
            _ = Task.Run(RunLoopAsync);
        }

        public Task<IReadOnlyList<Trade>> SubmitAsync(BookOrder order)
        {
            var tcs = new TaskCompletionSource<IReadOnlyList<Trade>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _commands.Writer.TryWrite(() =>
            {
                try
                {
                    tcs.SetResult(_book.Submit(order));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<bool> CancelAsync(Guid orderId)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _commands.Writer.TryWrite(() =>
            {
                try
                {
                    tcs.SetResult(_book.Cancel(orderId));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        private async Task RunLoopAsync()
        {
            await foreach (var command in _commands.Reader.ReadAllAsync())
            {
                command();
            }
        }
    }
}
