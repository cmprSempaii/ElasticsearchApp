using ElasticsearchApp.Models;

namespace ElasticsearchApp.Services;

public interface IElasticsearchService
{
    Task EnsureIndexAsync();
    Task BulkIndexAsync(IEnumerable<ArticleDocument> documents);
    Task<IEnumerable<ArticleDocument>> SearchAsync(string query);
}