using Microsoft.AspNetCore.Mvc;
using ElasticsearchApp.DTOs;
using ElasticsearchApp.Messages;
using ElasticsearchApp.Services;

namespace ElasticsearchApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IMessagePublisher _publisher;

    public ArticlesController(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        var message = new ArticleIndexMessage
        {
            Id = request.Id,
            Title = request.Title,
            Content = request.Content
        };

        await _publisher.PublishAsync(message, "article.index");

        // Возвращаем 202 Accepted, как требует задание
        return Accepted(new { message = "Статья принята в обработку", id = message.Id });
    }
}