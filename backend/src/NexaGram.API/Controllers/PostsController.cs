using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _posts;
    private readonly IEngagementService _engagement;

    public PostsController(IPostService posts, IEngagementService engagement)
    {
        _posts = posts;
        _engagement = engagement;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDto>> Create(CreatePostRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var post = await _posts.CreateAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetById(Guid id, CancellationToken ct)
    {
        var requestingUserId = TryGetUserId();
        var post = await _posts.GetByIdAsync(id, requestingUserId, ct);
        return Ok(post);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostDto>> Update(Guid id, UpdatePostRequest request, CancellationToken ct)
    {
        var post = await _posts.UpdateAsync(id, GetUserId(), request, ct);
        return Ok(post);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _posts.DeleteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    // ── Likes ──────────────────────────────────────────────────────────────

    [Authorize]
    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> Like(Guid id, CancellationToken ct)
    {
        await _engagement.LikePostAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/like")]
    public async Task<IActionResult> Unlike(Guid id, CancellationToken ct)
    {
        await _engagement.UnlikePostAsync(id, GetUserId(), ct);
        return NoContent();
    }

    // ── Saves ──────────────────────────────────────────────────────────────

    [Authorize]
    [HttpPost("{id:guid}/save")]
    public async Task<IActionResult> Save(Guid id, CancellationToken ct)
    {
        await _engagement.SavePostAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/save")]
    public async Task<IActionResult> Unsave(Guid id, CancellationToken ct)
    {
        await _engagement.UnsavePostAsync(id, GetUserId(), ct);
        return NoContent();
    }

    // ── Comments ───────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/comments")]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetComments(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _engagement.GetCommentsAsync(id, page, pageSize, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<CommentDto>> CreateComment(
        Guid id, CreateCommentRequest request, CancellationToken ct)
    {
        var comment = await _engagement.CreateCommentAsync(id, GetUserId(), request, ct);
        return Ok(comment);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private Guid? TryGetUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return value is null ? null : Guid.Parse(value);
    }
}
