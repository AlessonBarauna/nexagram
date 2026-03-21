using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IPostService
{
    Task<PostDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken ct = default);
    Task<PostDto> GetByIdAsync(Guid postId, Guid? requestingUserId, CancellationToken ct = default);
    Task<PostDto> UpdateAsync(Guid postId, Guid userId, UpdatePostRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid postId, Guid userId, CancellationToken ct = default);
    Task<PagedResult<PostDto>> GetUserPostsAsync(string username, Guid? requestingUserId, int page, int pageSize, CancellationToken ct = default);
}
