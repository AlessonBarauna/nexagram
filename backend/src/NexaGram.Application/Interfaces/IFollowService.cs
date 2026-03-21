using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IFollowService
{
    Task FollowAsync(Guid followerId, Guid followingId, CancellationToken ct = default);
    Task UnfollowAsync(Guid followerId, Guid followingId, CancellationToken ct = default);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followingId, CancellationToken ct = default);
    Task<PagedResult<UserSummaryDto>> GetSuggestionsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
