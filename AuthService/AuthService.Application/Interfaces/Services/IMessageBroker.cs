namespace AuthService.Application.Interfaces.Services
{
    public interface IMessageBroker
    {
        Task PublishAsync<T>(string queueName, T message);
    }
}
