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

    public PostsController(IPostService posts) => _posts = posts;

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

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private Guid? TryGetUserId()
    {
        var value = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return value is null ? null : Guid.Parse(value);
    }
}
