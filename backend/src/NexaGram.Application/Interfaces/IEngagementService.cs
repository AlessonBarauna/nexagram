using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IEngagementService
{
    // Likes
    Task LikePostAsync(Guid postId, Guid userId, CancellationToken ct = default);
    Task UnlikePostAsync(Guid postId, Guid userId, CancellationToken ct = default);

    // Saves
    Task SavePostAsync(Guid postId, Guid userId, CancellationToken ct = default);
    Task UnsavePostAsync(Guid postId, Guid userId, CancellationToken ct = default);

    // Comments
    Task<PagedResult<CommentDto>> GetCommentsAsync(Guid postId, int page, int pageSize, CancellationToken ct = default);
    Task<CommentDto> CreateCommentAsync(Guid postId, Guid userId, CreateCommentRequest request, CancellationToken ct = default);
    Task<CommentDto> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request, CancellationToken ct = default);
    Task DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default);
    Task LikeCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default);
    Task UnlikeCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default);
}
