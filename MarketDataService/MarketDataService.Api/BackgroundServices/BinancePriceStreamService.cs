using MarketDataService.Application.Models;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MarketDataService.Api.BackgroundServices
{
    public class BinancePriceStreamService : BackgroundService
    {
        private readonly ILogger<BinancePriceStreamService> _logger;

        public BinancePriceStreamService(ILogger<BinancePriceStreamService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var symbol = "btcusdt";
            var url = new Uri($"wss://stream.binance.com:9443/ws/{symbol}@ticker");

            using var socket = new ClientWebSocket();

            await socket.ConnectAsync(url, stoppingToken);

            var buffer = new byte[8192];

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = await socket.ReceiveAsync(buffer, stoppingToken);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var ticker = JsonSerializer.Deserialize<BinanceTicker>(json);

                if (ticker == null)
                    continue;

                _logger.LogInformation(
                    "Price update {Symbol}: Last={LastPrice}, Bid={Bid}, Ask={Ask}",
                    ticker.Symbol,
                    ticker.LastPrice,
                    ticker.BidPrice,
                    ticker.AskPrice);

                // TODO:
                // Save latest price to Redis/in-memory cache
                // Publish PriceUpdatedEvent
                // Push to Angular using SignalR
            }
        }
    }


}
