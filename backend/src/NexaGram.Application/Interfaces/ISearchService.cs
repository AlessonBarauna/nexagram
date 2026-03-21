using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface ISearchService
{
    /// <summary>Global search across users, hashtags and posts.</summary>
    Task<SearchResultDto> SearchAsync(string query, int limit, CancellationToken ct = default);

    /// <summary>Posts tagged with a specific hashtag.</summary>
    Task<PagedResult<PostDto>> GetPostsByHashtagAsync(string hashtag, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Trending hashtags by post count.</summary>
    Task<IReadOnlyList<HashtagDto>> GetTrendingHashtagsAsync(int limit, CancellationToken ct = default);

    /// <summary>Sync hashtags from a post caption (upsert).</summary>
    Task SyncPostHashtagsAsync(Guid postId, string? caption, CancellationToken ct = default);
}
