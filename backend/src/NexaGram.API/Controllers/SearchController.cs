using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _search;

    public SearchController(ISearchService search) => _search = search;

    [HttpGet]
    public async Task<ActionResult<SearchResultDto>> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required.");

        var result = await _search.SearchAsync(q, Math.Clamp(limit, 1, 50), ct);
        return Ok(result);
    }

    [HttpGet("hashtags/{hashtag}/posts")]
    public async Task<ActionResult<PagedResult<PostDto>>> GetByHashtag(
        string hashtag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _search.GetPostsByHashtagAsync(hashtag, page, pageSize, ct);
        return Ok(result);
    }

    [HttpGet("hashtags/trending")]
    public async Task<ActionResult<IReadOnlyList<HashtagDto>>> GetTrending(
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var result = await _search.GetTrendingHashtagsAsync(Math.Clamp(limit, 1, 50), ct);
        return Ok(result);
    }
}
