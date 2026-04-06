using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticsearchApp.Models;

namespace ElasticsearchApp.Services;

public class ElasticsearchService : IElasticsearchService
{
    private const string IndexName = "articles";
    private readonly ElasticsearchClient _client;

    public ElasticsearchService(ElasticsearchClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task EnsureIndexAsync()
    {
        var existsResponse = await _client.Indices.ExistsAsync(IndexName);
        if (existsResponse.Exists)
            return;

        var createResponse = await _client.Indices.CreateAsync(IndexName);
        if (!createResponse.IsValidResponse)
            throw new Exception($"Ошибка создания индекса: {createResponse.DebugInformation}");
    }

    public async Task BulkIndexAsync(IEnumerable<ArticleDocument> documents)
    {
        if (documents == null) throw new ArgumentNullException(nameof(documents));

        var bulkDescriptor = new BulkRequestDescriptor();
        bulkDescriptor.IndexMany(documents, (op, doc) => op
            .Index(IndexName)
            .Id(doc.Id.ToString())
        );

        var response = await _client.BulkAsync(bulkDescriptor);
        if (!response.IsValidResponse)
            throw new Exception($"Ошибка Bulk индексации: {response.DebugInformation}");
    }

    public async Task<IEnumerable<ArticleDocument>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<ArticleDocument>();

        var searchResponse = await _client.SearchAsync<ArticleDocument>(s => s
            .Index(IndexName)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sh => sh.Match(m => m.Field(f => f.Title).Query(query).Boost(2.0f)),
                        sh => sh.Match(m => m.Field(f => f.Content).Query(query))
                    )
                )
            )
            .Size(20)
        );

        if (!searchResponse.IsValidResponse)
            return Enumerable.Empty<ArticleDocument>();

        return searchResponse.Documents;
    }
}