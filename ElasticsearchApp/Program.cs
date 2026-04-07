using Elastic.Clients.Elasticsearch;
using ElasticsearchApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов ASP.NET Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Регистрация ElasticsearchClient с настройками диагностики
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
        .DefaultIndex("articles")
        .EnableDebugMode()                // Позволяет увидеть детали запросов/ответов в консоли
        .DisableDirectStreaming();        // Захватывает тело запроса/ответа для отладки
    return new ElasticsearchClient(settings);
});

// Регистрация собственных сервисов
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<ElasticsearchIndexingBackgroundService>();

var app = builder.Build();

// Настройка конвейера HTTP-запросов
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Создание индекса Elasticsearch при старте приложения
using (var scope = app.Services.CreateScope())
{
    var elasticService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

    // Ждём 5 секунд, чтобы Elasticsearch успел полностью подняться (особенно в Docker)
    await Task.Delay(5000);

    try
    {
        await elasticService.EnsureIndexAsync();
        Console.WriteLine("Индекс 'articles' успешно создан или уже существует.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"НЕ УДАЛОСЬ создать индекс: {ex.Message}");
        Console.WriteLine("Приложение продолжит работу, но индексация может быть недоступна.");
        // Приложение не падает, чтобы можно было вручную создать индекс через curl или Kibana
    }
}

app.Run();