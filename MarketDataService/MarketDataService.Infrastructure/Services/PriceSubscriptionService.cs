using MarketDataService.Application.Interfaces.Services;
using MarketDataService.Application.Models;
using MarketDataService.Domain.Helpers;
using MarketDataService.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

namespace MarketDataService.Infrastructure.Services
{
    public sealed class PriceSubscriptionService : ISubscriptionService, IDisposable
    {
        private const int BufferSize = 8192;
        private const int MaxTickerMessageBytes = 64 * 1024;
        private static readonly TimeSpan InitialReconnectDelay = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan MaxReconnectDelay = TimeSpan.FromSeconds(30);

        private readonly ConcurrentDictionary<string, Lazy<Task>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);
        private readonly CancellationTokenSource _lifetimeCts = new();
        private readonly IPriceCache _priceCache;
        private readonly ILogger<PriceSubscriptionService> _logger;
        private readonly IHubContext<MarketDataHub> _hubContext;

        public PriceSubscriptionService(IPriceCache priceCache, ILogger<PriceSubscriptionService> logger, IHubContext<MarketDataHub> hubContext)
        {
            _priceCache = priceCache;
            _logger = logger;
            _hubContext = hubContext;
        }

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();
        }

        public Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default)
        {
            if (!TradingSymbol.TryNormalize(symbol, out var normalizedSymbol))
            {
                throw new ArgumentException("Symbol must contain 1 to 32 ASCII letters or digits.", nameof(symbol));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var subscription = _subscriptions.GetOrAdd(normalizedSymbol,
                static (key, service) => new Lazy<Task>(() =>
                    service.RunSubscriptionAsync(key),
                    LazyThreadSafetyMode.ExecutionAndPublication),
                this);

            _ = subscription.Value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Keeps a symbol's Binance feed alive for the life of the service: a dropped/closed socket
        /// (idle timeout, network blip, Binance-side rate limiting) is reconnected with backoff rather
        /// than being left permanently dead until some caller happens to re-subscribe.
        /// </summary>
        private async Task RunSubscriptionAsync(string symbol)
        {
            var stoppingToken = _lifetimeCts.Token;
            var reconnectDelay = InitialReconnectDelay;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunSingleConnectionAsync(symbol, stoppingToken);
                    reconnectDelay = InitialReconnectDelay;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (WebSocketException ex)
                {
                    _logger.LogWarning(ex, "Binance ticker subscription for {Symbol} disconnected. Reconnecting in {Delay}s", symbol, reconnectDelay.TotalSeconds);
                }
                catch (InvalidDataException ex)
                {
                    _logger.LogWarning(ex, "Binance ticker subscription for {Symbol} disconnected. Reconnecting in {Delay}s", symbol, reconnectDelay.TotalSeconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected Binance ticker subscription failure for {Symbol}. Reconnecting in {Delay}s", symbol, reconnectDelay.TotalSeconds);
                }

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await Task.Delay(reconnectDelay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                reconnectDelay = TimeSpan.FromSeconds(Math.Min(reconnectDelay.TotalSeconds * 2, MaxReconnectDelay.TotalSeconds));
            }

            _subscriptions.TryRemove(symbol, out _);
        }

        private async Task RunSingleConnectionAsync(string symbol, CancellationToken stoppingToken)
        {
            var streamSymbol = symbol.ToLowerInvariant();
            var url = new Uri($"wss://stream.binance.com:9443/ws/{streamSymbol}@ticker");

            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(url, stoppingToken);

            _logger.LogInformation("Started Binance ticker subscription for {Symbol}", symbol);

            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            {
                while (!stoppingToken.IsCancellationRequested && socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(buffer, stoppingToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    BinanceTicker? ticker;

                    try
                    {
                        ticker = result.EndOfMessage
                            ? JsonSerializer.Deserialize<BinanceTicker>(buffer.AsSpan(0, result.Count))
                            : await ReceiveFragmentedTickerAsync(socket, buffer, result, stoppingToken);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Received invalid Binance ticker payload for {Symbol}", symbol);
                        continue;
                    }

                    if (ticker == null || !TradingSymbol.TryNormalize(ticker.Symbol, out var tickerSymbol))
                    {
                        continue;
                    }

                    if (!string.Equals(symbol, tickerSymbol, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    CryptoCurrencyPrice price;

                    try
                    {
                        price = new CryptoCurrencyPrice(
                            tickerSymbol,
                            ticker.LastPrice,
                            ticker.BidPrice,
                            ticker.AskPrice,
                            DateTimeOffset.FromUnixTimeMilliseconds(ticker.E));
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogWarning(ex, "Received Binance ticker payload with invalid prices for {Symbol}", symbol);
                        continue;
                    }
                    catch (OverflowException ex)
                    {
                        _logger.LogWarning(ex, "Received Binance ticker payload with out-of-range prices for {Symbol}", symbol);
                        continue;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        _logger.LogWarning(ex, "Received Binance ticker payload with out-of-range event time for {Symbol}", symbol);
                        continue;
                    }

                    _priceCache.Set(price);

                    await _hubContext.Clients.Group(price.Symbol).SendAsync("PriceUpdated", price);

                    _logger.LogInformation(
                        "Price update {Symbol}: Last={LastPrice}, Bid={Bid}, Ask={Ask}",
                        price.Symbol,
                        price.LastPrice,
                        price.BidPrice,
                        price.AskPrice);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static async Task<BinanceTicker?> ReceiveFragmentedTickerAsync(ClientWebSocket socket, byte[] buffer, WebSocketReceiveResult firstResult, CancellationToken cancellationToken)
        {
            var writer = new ArrayBufferWriter<byte>(BufferSize);
            var totalBytes = firstResult.Count;
            writer.Write(buffer.AsSpan(0, firstResult.Count));

            var result = firstResult;

            while (!result.EndOfMessage)
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return null;
                }

                totalBytes += result.Count;

                if (totalBytes > MaxTickerMessageBytes)
                {
                    throw new InvalidDataException("Binance ticker payload exceeded the maximum allowed size.");
                }

                writer.Write(buffer.AsSpan(0, result.Count));
            }

            return JsonSerializer.Deserialize<BinanceTicker>(writer.WrittenSpan);
        }
    }
}
