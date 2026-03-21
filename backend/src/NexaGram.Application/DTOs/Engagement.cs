namespace NexaGram.Application.DTOs;

public record CommentDto(
    Guid Id,
    UserSummaryDto Author,
    string Content,
    int LikeCount,
    bool IsPinned,
    Guid? ParentId,
    IReadOnlyList<CommentDto> Replies,
    DateTime CreatedAt);

public record CreateCommentRequest(string Content, Guid? ParentId = null);

public record UpdateCommentRequest(string Content);
