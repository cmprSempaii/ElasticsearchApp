using ElasticsearchApp.Models;
using ElasticsearchApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IElasticsearchService _elasticService;

    public DocumentsController(IElasticsearchService elasticService)
    {
        _elasticService = elasticService;
    }

    [HttpPost("load")]
    public async Task<IActionResult> LoadTestData()
    {
        var testDocuments = new List<ArticleDocument>
        {
            new() { Title = "Введение в Elasticsearch", Content = "Elasticsearch — это распределённая поисковая система." },
            new() { Title = "ASP.NET Core и поиск", Content = "Интеграция Elasticsearch в ASP.NET Core даёт мощный полнотекстовый поиск." },
            new() { Title = "Полнотекстовый поиск", Content = "Multi-match запрос ищет по нескольким полям сразу." },
            new() { Title = "Docker для разработки", Content = "Запуск Elasticsearch в Docker упрощает локальную разработку." }
        };

        await _elasticService.BulkIndexAsync(testDocuments);
        return Ok($"Загружено {testDocuments.Count} документов.");
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Параметр query обязателен.");

        var results = await _elasticService.SearchAsync(query);
        return Ok(results);
    }
}