using Elastic.Clients.Elasticsearch;
using ElasticsearchApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключение к Elasticsearch (адрес http://localhost:9200)
builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
        .DefaultIndex("articles");
    return new ElasticsearchClient(settings);
});

builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Создаём индекс при старте приложения
using (var scope = app.Services.CreateScope())
{
    var elasticService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
    await elasticService.EnsureIndexAsync();
}

app.Run();