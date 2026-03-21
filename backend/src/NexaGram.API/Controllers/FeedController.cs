using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/feed")]
public class FeedController : ControllerBase
{
    private readonly IFeedService _feed;

    public FeedController(IFeedService feed) => _feed = feed;

    /// <summary>Personal feed — posts from followed users + own posts.</summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<PagedResult<PostDto>>> GetPersonal(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await _feed.GetPersonalFeedAsync(userId, page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Explore feed — trending public posts.</summary>
    [HttpGet("explore")]
    public async Task<ActionResult<PagedResult<PostDto>>> GetExplore(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var userId = sub is null ? (Guid?)null : Guid.Parse(sub);
        var result = await _feed.GetExploreFeedAsync(userId, page, pageSize, ct);
        return Ok(result);
    }
}
