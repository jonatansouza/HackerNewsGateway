using HackerNewsGateway.Domain.Entities;
using HackerNewsGatewayApi.Cache;

namespace HackerNewsGateway.Tests;

public class StoryCacheTests
{
    private static Story MakeStory(int score) =>
        new("Title", "http://uri", "user", DateTimeOffset.UtcNow, score, 0);

    [Fact]
    public void IsEmpty_WhenNew_ReturnsTrue()
    {
        var cache = new StoryCache();
        Assert.True(cache.IsEmpty);
    }

    [Fact]
    public void IsEmpty_AfterReplace_ReturnsFalse()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(100)]);
        Assert.False(cache.IsEmpty);
    }

    [Fact]
    public void Take_ReturnsRequestedCount()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(1), MakeStory(2), MakeStory(3)]);
        Assert.Equal(2, cache.Take(2).Count());
    }

    [Fact]
    public void Take_WhenNExceedsCount_ReturnsAll()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(1), MakeStory(2)]);
        Assert.Equal(2, cache.Take(100).Count());
    }

    [Fact]
    public void Replace_OverwritesPreviousData()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(100)]);
        cache.Replace([MakeStory(200), MakeStory(300)]);
        Assert.Equal(2, cache.Take(10).Count());
    }
}
