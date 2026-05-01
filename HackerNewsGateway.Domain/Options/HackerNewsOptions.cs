namespace HackerNewsGateway.Domain.Options;

public sealed class HackerNewsOptions
{
    public string BaseUrl { get; init; } = "https://hacker-news.firebaseio.com";
    public int SyncIntervalMinutes { get; init; } = 5;
    public int TimeoutSeconds { get; init; } = 10;
    public int MaxParallelRequests { get; init; } = 20;
    public int MaxStories { get; init; } = 100;
}
