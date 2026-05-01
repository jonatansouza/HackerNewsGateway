using HackerNewsGateway.Domain.Entities;
using HackerNewsGateway.Domain.Options;
using HackerNewsGateway.Domain.ValueObjects;
using HackerNewsGatewayApi.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HackerNewsGatewayApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class StoriesController(IStoryCache cache, IOptions<HackerNewsOptions> options) : ControllerBase
{
    [HttpGet("{n:int}")]
    [ProducesResponseType(typeof(IEnumerable<Story>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<IEnumerable<Story>> Get(int n)
    {
        if (!StoryCount.Config(options.Value.MaxStories).TryCreate(n, out var count, out var error))
            return BadRequest(error);

        if (cache.IsEmpty)
        {
            Response.Headers.Append("Retry-After", "10");
            return StatusCode(503, "Cache is warming up. Please retry shortly.");
        }

        return Ok(cache.Take(count.Value));
    }
}
