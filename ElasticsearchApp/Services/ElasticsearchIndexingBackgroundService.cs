using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Elastic.Clients.Elasticsearch;
using ElasticsearchApp.Messages;
using ElasticsearchApp.Models;

namespace ElasticsearchApp.Services;

public class ElasticsearchIndexingBackgroundService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _queueName = "article_index_queue";

    public ElasticsearchIndexingBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<ArticleIndexMessage>(messageJson);

            if (message != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var elasticClient = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();
                await IndexDocumentAsync(elasticClient, message);
            }

            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        await Task.CompletedTask;
    }

    private async Task IndexDocumentAsync(ElasticsearchClient client, ArticleIndexMessage message)
    {
        var indexName = "articles";
        var doc = new ArticleDocument
        {
            Id = message.Id,
            Title = message.Title,
            Content = message.Content
        };

        var response = await client.IndexAsync(doc, idx => idx
            .Index(indexName)
            .Id(doc.Id.ToString())
            .OpType(OpType.Index));

        if (!response.IsValidResponse)
        {
            Console.WriteLine($"Ошибка индексации: {response.DebugInformation}");
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}