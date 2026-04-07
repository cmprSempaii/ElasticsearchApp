namespace ElasticsearchApp.Services;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string routingKey);
}