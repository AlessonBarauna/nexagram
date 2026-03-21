using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IStoryService
{
    Task<StoryDto> CreateAsync(Guid userId, CreateStoryRequest request, CancellationToken ct = default);
    Task<StoryDto> GetByIdAsync(Guid storyId, Guid? requestingUserId, CancellationToken ct = default);
    Task DeleteAsync(Guid storyId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<StoryFeedItemDto>> GetFeedAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<StoryDto>> GetUserStoriesAsync(string username, Guid? requestingUserId, CancellationToken ct = default);
    Task ViewAsync(Guid storyId, Guid userId, CancellationToken ct = default);
    Task<PagedResult<StoryViewerDto>> GetViewersAsync(Guid storyId, Guid userId, int page, int pageSize, CancellationToken ct = default);
}
