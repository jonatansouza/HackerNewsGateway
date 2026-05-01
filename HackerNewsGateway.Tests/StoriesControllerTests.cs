using HackerNewsGateway.Domain.Entities;
using HackerNewsGateway.Domain.Options;
using HackerNewsGatewayApi.Cache;
using HackerNewsGatewayApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HackerNewsGateway.Tests;

public class StoriesControllerTests
{
    private static readonly IOptions<HackerNewsOptions> DefaultOptions =
        Microsoft.Extensions.Options.Options.Create(new HackerNewsOptions());

    private static Story MakeStory(int score) =>
        new("Title", "http://uri", "user", DateTimeOffset.UtcNow, score, 0);

    private static StoriesController CreateController(IStoryCache cache, IOptions<HackerNewsOptions>? options = null)
    {
        var controller = new StoriesController(cache, options ?? DefaultOptions);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public void Get_NIsZero_ReturnsBadRequest()
    {
        var result = CreateController(new StoryCache()).Get(0);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Get_NIsNegative_ReturnsBadRequest()
    {
        var result = CreateController(new StoryCache()).Get(-1);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Get_NExceedsMaxStories_ReturnsBadRequest()
    {
        var result = CreateController(new StoryCache()).Get(DefaultOptions.Value.MaxStories + 1);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Get_NExceedsCustomMax_ReturnsBadRequest()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new HackerNewsOptions { MaxStories = 10 });
        var result = CreateController(new StoryCache(), options).Get(11);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Get_CacheEmpty_Returns503()
    {
        var result = CreateController(new StoryCache()).Get(5);
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, statusResult.StatusCode);
    }

    [Fact]
    public void Get_CacheEmpty_SetsRetryAfterHeader()
    {
        var controller = CreateController(new StoryCache());
        controller.Get(5);
        Assert.True(controller.Response.Headers.ContainsKey("Retry-After"));
    }

    [Fact]
    public void Get_ValidRequest_Returns200()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(100), MakeStory(200), MakeStory(300)]);

        var result = CreateController(cache).Get(2);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void Get_ValidRequest_ReturnsExactNStories()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(100), MakeStory(200), MakeStory(300)]);

        var result = CreateController(cache).Get(2);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var stories = Assert.IsAssignableFrom<IEnumerable<Story>>(ok.Value);
        Assert.Equal(2, stories.Count());
    }
}
