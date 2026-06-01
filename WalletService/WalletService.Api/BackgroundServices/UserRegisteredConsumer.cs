using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedLibrary.EventDriven.Models;
using System.Text;
using System.Text.Json;

namespace WalletService.Api.BackgroundServices
{
    public class UserRegisteredConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRegisteredConsumer> _logger;

        public UserRegisteredConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<UserRegisteredConsumer> logger) {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            var factory = new ConnectionFactory {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost"
            };

            IConnection? connection = null;
            while (connection == null) {
                try {
                    connection = await factory.CreateConnectionAsync();
                } catch (Exception ex) {
                    _logger?.LogError(ex, "Failed to connect to RabbitMQ. Retrying in 1 minute...");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "wallet.user.registered", durable: true, exclusive: false, autoDelete: false);
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) => {
                try {
                    _logger?.LogInformation("Received UserRegisteredEvent with delivery tag {DeliveryTag}", ea.DeliveryTag);

                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<UserRegisteredEvent>(json);

                    using var scope = _scopeFactory.CreateScope();

                    var userWalletService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.Services.IUserWalletService>();
                    var userWallet = await userWalletService.GetUserWallet(message!.UserId);
                    if (userWallet == null)
                        throw new Exception($"User wallet not found for user {message.UserId}");

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    _logger?.LogInformation("Processed UserRegisteredEvent for user {UserId} and acknowledged message", message.UserId);
                } catch (Exception ex) {
                    _logger?.LogError(ex, "Error processing UserRegisteredEvent");
                }
            };

            await channel.BasicConsumeAsync(queue: "wallet.user.registered", autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
