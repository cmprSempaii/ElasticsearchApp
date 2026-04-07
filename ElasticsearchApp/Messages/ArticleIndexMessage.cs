namespace ElasticsearchApp.Messages;

public class ArticleIndexMessage
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}