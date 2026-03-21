using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/stories")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _stories;

    public StoriesController(IStoryService stories) => _stories = stories;

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<StoryDto>> Create(CreateStoryRequest request, CancellationToken ct)
    {
        var story = await _stories.CreateAsync(GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = story.Id }, story);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var story = await _stories.GetByIdAsync(id, TryGetUserId(), ct);
        return Ok(story);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _stories.DeleteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("feed")]
    public async Task<ActionResult<IReadOnlyList<StoryFeedItemDto>>> GetFeed(CancellationToken ct)
    {
        var result = await _stories.GetFeedAsync(GetUserId(), ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> View(Guid id, CancellationToken ct)
    {
        await _stories.ViewAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{id:guid}/views")]
    public async Task<ActionResult<PagedResult<StoryViewerDto>>> GetViewers(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await _stories.GetViewersAsync(id, GetUserId(), page, pageSize, ct);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private Guid? TryGetUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return value is null ? null : Guid.Parse(value);
    }
}
