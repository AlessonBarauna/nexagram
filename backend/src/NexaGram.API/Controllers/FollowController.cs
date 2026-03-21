using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class FollowController : ControllerBase
{
    private readonly IFollowService _follow;

    public FollowController(IFollowService follow) => _follow = follow;

    [HttpPost("{username}/follow")]
    public async Task<IActionResult> Follow(string username, [FromServices] IUserService users, CancellationToken ct)
    {
        var target = await users.GetProfileAsync(username, ct);
        await _follow.FollowAsync(GetUserId(), target.Id, ct);
        return NoContent();
    }

    [HttpDelete("{username}/follow")]
    public async Task<IActionResult> Unfollow(string username, [FromServices] IUserService users, CancellationToken ct)
    {
        var target = await users.GetProfileAsync(username, ct);
        await _follow.UnfollowAsync(GetUserId(), target.Id, ct);
        return NoContent();
    }

    [HttpGet("{username}/is-following")]
    public async Task<ActionResult<bool>> IsFollowing(string username, [FromServices] IUserService users, CancellationToken ct)
    {
        var target = await users.GetProfileAsync(username, ct);
        var result = await _follow.IsFollowingAsync(GetUserId(), target.Id, ct);
        return Ok(result);
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> Suggestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _follow.GetSuggestionsAsync(GetUserId(), page, pageSize, ct);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
