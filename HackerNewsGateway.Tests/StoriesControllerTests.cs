using HackerNewsGateway.Domain.Entities;
using HackerNewsGatewayApi.Cache;
using HackerNewsGatewayApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsGateway.Tests;

public class StoriesControllerTests
{
    private static Story MakeStory(int score) =>
        new("Title", "http://uri", "user", DateTimeOffset.UtcNow, score, 0);

    private static StoriesController CreateController(StoryCache cache)
    {
        var controller = new StoriesController(cache);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public void Get_NIsZero_ReturnsBadRequest()
    {
        var controller = CreateController(new StoryCache());
        var result = controller.Get(0);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Get_NIsNegative_ReturnsBadRequest()
    {
        var controller = CreateController(new StoryCache());
        var result = controller.Get(-1);
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Get_CacheEmpty_Returns503()
    {
        var controller = CreateController(new StoryCache());
        var result = controller.Get(5);
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
        var controller = CreateController(cache);

        var result = controller.Get(2);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void Get_ValidRequest_ReturnsExactNStories()
    {
        var cache = new StoryCache();
        cache.Replace([MakeStory(100), MakeStory(200), MakeStory(300)]);
        var controller = CreateController(cache);

        var result = controller.Get(2);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var stories = Assert.IsAssignableFrom<IEnumerable<Story>>(ok.Value);
        Assert.Equal(2, stories.Count());
    }
}
