using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IFeedService
{
    /// <summary>Personal feed: posts from followed users, newest first.</summary>
    Task<PagedResult<PostDto>> GetPersonalFeedAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Explore feed: trending public posts, ordered by engagement.</summary>
    Task<PagedResult<PostDto>> GetExploreFeedAsync(Guid? userId, int page, int pageSize, CancellationToken ct = default);
}
