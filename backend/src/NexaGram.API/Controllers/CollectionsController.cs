using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/collections")]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collections;

    public CollectionsController(ICollectionService collections) => _collections = collections;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CollectionDto>>> GetMine(CancellationToken ct)
    {
        var result = await _collections.GetUserCollectionsAsync(GetUserId(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/posts")]
    public async Task<ActionResult<PagedResult<PostDto>>> GetPosts(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _collections.GetCollectionPostsAsync(id, GetUserId(), page, pageSize, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CollectionDto>> Create(CreateCollectionRequest request, CancellationToken ct)
    {
        var result = await _collections.CreateAsync(GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetMine), result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CollectionDto>> Update(Guid id, UpdateCollectionRequest request, CancellationToken ct)
    {
        var result = await _collections.UpdateAsync(id, GetUserId(), request, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _collections.DeleteAsync(id, GetUserId(), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/posts/{postId:guid}")]
    public async Task<IActionResult> AddPost(Guid id, Guid postId, CancellationToken ct)
    {
        await _collections.AddPostAsync(id, postId, GetUserId(), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/posts/{postId:guid}")]
    public async Task<IActionResult> RemovePost(Guid id, Guid postId, CancellationToken ct)
    {
        await _collections.RemovePostAsync(id, postId, GetUserId(), ct);
        return NoContent();
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
