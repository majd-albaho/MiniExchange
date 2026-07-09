using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedLibrary.EventDriven.Models;
using System.Text;
using System.Text.Json;
using TradingService.Application.Interfaces.Services;

namespace TradingService.Api.BackgroundServices
{
    public class TradeExecutedConsumer : BackgroundService
    {
        private const string QueueName = "trading.trade.executed";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TradeExecutedConsumer> _logger;

        public TradeExecutedConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<TradeExecutedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost"
            };

            IConnection? connection = null;
            while (connection == null)
            {
                try
                {
                    connection = await factory.CreateConnectionAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to connect to RabbitMQ. Retrying in 1 minute...");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    _logger?.LogInformation("Received TradeExecutedEvent with delivery tag {DeliveryTag}", ea.DeliveryTag);

                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<TradeExecutedEvent>(json);

                    using var scope = _scopeFactory.CreateScope();

                    var tradeSettlementService = scope.ServiceProvider.GetRequiredService<ITradeSettlementService>();
                    await tradeSettlementService.ApplyTradeAsync(message!, stoppingToken);

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    _logger?.LogInformation("Processed TradeExecutedEvent {TradeId} and acknowledged message", message!.TradeId);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error processing TradeExecutedEvent");
                }
            };

            await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
