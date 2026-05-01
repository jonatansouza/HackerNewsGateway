using HackerNewsGatewayApi.Cache;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsGatewayApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class StoriesController(StoryCache cache) : ControllerBase
{
    [HttpGet("{n:int}")]
    public IActionResult Get(int n)
    {
        if (n < 1)
            return BadRequest("n must be greater than 0.");

        if (cache.IsEmpty)
        {
            Response.Headers.Append("Retry-After", "10");
            return StatusCode(503, "Cache is warming up. Please retry shortly.");
        }

        return Ok(cache.Take(n));
    }
}
