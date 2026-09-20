using HackerNewsApi.Models;
using HackerNewsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IHackerNewsService _service;

    public StoriesController(IHackerNewsService service)
    {
        _service = service;
    }

    [HttpGet("best/{n:int}")]
    [ProducesResponseType(
        typeof(IReadOnlyList<StoryResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBestStories(
        int n,
        CancellationToken cancellationToken)
    {
        if (n <= 0)
        {
            return BadRequest(
                "The number of stories must be greater than zero.");
        }

        if (n > 100)
        {
            return BadRequest(
                "The maximum number of stories is 100.");
        }

        var stories = await _service.GetBestStoriesAsync(
            n,
            cancellationToken);

        return Ok(stories);
    }
}