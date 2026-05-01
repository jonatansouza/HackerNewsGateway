using System.Net;
using System.Text;
using HackerNewsGateway.Infrastructure.Http;

namespace HackerNewsGateway.Tests;

public class HackerNewsClientTests
{
    private static HackerNewsClient CreateClient(HttpResponseMessage response)
    {
        var handler = new MockHttpMessageHandler(response);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com")
        };
        return new HackerNewsClient(httpClient);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    [Fact]
    public async Task GetBestStoryIdsAsync_ReturnsIds()
    {
        var client = CreateClient(JsonResponse("[1, 2, 3]"));

        var ids = await client.GetBestStoryIdsAsync();

        Assert.Equal([1, 2, 3], ids);
    }

    [Fact]
    public async Task GetStoryAsync_ValidStory_MapsFieldsCorrectly()
    {
        var json = """
            {"id":1,"title":"Test Title","url":"http://test.com","by":"user1",
             "time":1175714200,"score":111,"descendants":71}
            """;
        var client = CreateClient(JsonResponse(json));

        var story = await client.GetStoryAsync(1);

        Assert.NotNull(story);
        Assert.Equal("Test Title", story.Title);
        Assert.Equal("http://test.com", story.Uri);
        Assert.Equal("user1", story.PostedBy);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1175714200), story.Time);
        Assert.Equal(111, story.Score);
        Assert.Equal(71, story.CommentCount);
    }

    [Fact]
    public async Task GetStoryAsync_MissingUrl_MapsToEmptyString()
    {
        var json = """{"id":1,"title":"Ask HN","by":"user","time":1175714200,"score":50,"descendants":10}""";
        var client = CreateClient(JsonResponse(json));

        var story = await client.GetStoryAsync(1);

        Assert.NotNull(story);
        Assert.Equal(string.Empty, story.Uri);
    }

    [Fact]
    public async Task GetStoryAsync_DeletedItem_ReturnsNull()
    {
        var json = """{"id":1,"deleted":true}""";
        var client = CreateClient(JsonResponse(json));

        var story = await client.GetStoryAsync(1);

        Assert.Null(story);
    }

    [Fact]
    public async Task GetStoryAsync_HttpError_ReturnsNull()
    {
        var client = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound));

        var story = await client.GetStoryAsync(999);

        Assert.Null(story);
    }
}

file sealed class MockHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
        Task.FromResult(response);
}
