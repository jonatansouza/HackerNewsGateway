using HackerNewsGateway.Domain.Entities;
using HackerNewsGatewayApi.Cache;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsGatewayApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class StoriesController(IStoryCache cache) : ControllerBase
{
    [HttpGet("{n:int}")]
    [ProducesResponseType(typeof(IEnumerable<Story>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<IEnumerable<Story>> Get(int n)
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
