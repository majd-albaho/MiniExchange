using AuthService.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Application.Services
{
    public class RabbitMqMessageBroker : IMessageBroker
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqMessageBroker(IConfiguration configuration) {
            _factory = new ConnectionFactory {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost"
            };
        }

        public async Task PublishAsync<T>(string queueName, T message) {
            using var connection = await _factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var properties = new BasicProperties { Persistent = true };
            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, mandatory: false, basicProperties: properties, body: body);
        }
    }
}
