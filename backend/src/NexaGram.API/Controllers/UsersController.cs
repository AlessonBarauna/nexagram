using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IPostService _posts;
    private readonly IStoryService _stories;

    public UsersController(IUserService users, IPostService posts, IStoryService stories)
    {
        _users = users;
        _posts = posts;
        _stories = stories;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<UserProfileDto>> GetProfile(string username, CancellationToken ct)
    {
        var profile = await _users.GetProfileAsync(username, ct);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var profile = await _users.UpdateProfileAsync(userId, request, ct);
        return Ok(profile);
    }

    [HttpGet("{username}/followers")]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetFollowers(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _users.GetFollowersAsync(username, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{username}/following")]
    public async Task<ActionResult<PagedResult<UserSummaryDto>>> GetFollowing(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _users.GetFollowingAsync(username, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{username}/posts")]
    public async Task<ActionResult<PagedResult<PostDto>>> GetPosts(
        string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var requestingUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) is { } s ? Guid.Parse(s) : (Guid?)null;
        var result = await _posts.GetUserPostsAsync(username, requestingUserId, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("{username}/stories")]
    public async Task<ActionResult<IReadOnlyList<StoryDto>>> GetStories(string username, CancellationToken ct)
    {
        var requestingUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) is { } s ? Guid.Parse(s) : (Guid?)null;
        var result = await _stories.GetUserStoriesAsync(username, requestingUserId, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("me/avatar")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<ActionResult<UserProfileDto>> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType))
            return BadRequest("Allowed formats: jpeg, png, webp.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);

        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var profile = await _users.UpdateAvatarAsync(userId, ms.ToArray(), file.ContentType, ct);
        return Ok(profile);
    }
}
