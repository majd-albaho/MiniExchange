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

        public UserRegisteredConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost"
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "wallet.user.registered", durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<UserRegisteredEvent>(json);

                    using var scope = _scopeFactory.CreateScope();

                    //var dbContext = scope.ServiceProvider.GetRequiredService<WalletService>();
                    //var exists = await dbContext.Wallets.AnyAsync(x => x.UserId == message!.UserId);

                    //if (!exists) {
                    //    dbContext.Wallets.Add(new Wallet(message.UserId));
                    //    await dbContext.SaveChangesAsync();
                    //}

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    // no ack = retry later
                }
            };

            await channel.BasicConsumeAsync(queue: "wallet.user.registered", autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
