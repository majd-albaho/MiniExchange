using MarketDataService.Application.Models;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;

namespace MarketDataService.Api.Services
{
    public sealed class BinancePriceSubscriptionService : ISubscriptionService
    {
        private const int BufferSize = 8192;
        private const int MaxTickerMessageBytes = 64 * 1024;

        private readonly ConcurrentDictionary<string, Lazy<Task>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);
        private readonly IPriceCache _priceCache;
        private readonly ILogger<BinancePriceSubscriptionService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public BinancePriceSubscriptionService(IPriceCache priceCache, ILogger<BinancePriceSubscriptionService> logger, IHostApplicationLifetime applicationLifetime)
        {
            _priceCache = priceCache;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        public Task SubscribeAsync(string symbol, CancellationToken cancellationToken = default)
        {
            if (!BinanceSymbol.TryNormalize(symbol, out var normalizedSymbol))
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

        private async Task RunSubscriptionAsync(string symbol)
        {
            var stoppingToken = _applicationLifetime.ApplicationStopping;

            try
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

                        if (ticker == null || !BinanceSymbol.TryNormalize(ticker.Symbol, out var tickerSymbol))
                        {
                            continue;
                        }

                        if (!string.Equals(symbol, tickerSymbol, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        BinancePrice price;

                        try
                        {
                            price = new BinancePrice(
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
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, "Binance ticker subscription ended for {Symbol}", symbol);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "Binance ticker subscription ended for {Symbol}", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Binance ticker subscription failure for {Symbol}", symbol);
            }
            finally
            {
                _subscriptions.TryRemove(symbol, out _);
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
