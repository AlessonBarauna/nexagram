using NexaGram.Domain.Entities;

namespace NexaGram.Application.DTOs;

public record MediaItem(string Url, string MimeType);

public record MediaItemInput(string Key, string MimeType);

public record CreatePostRequest(
    string? Caption,
    MediaItemInput[] Media,
    PostVisibility Visibility = PostVisibility.Public,
    string? Location = null,
    DateTime? ScheduledFor = null,
    PostStatus Status = PostStatus.Published);

public record UpdatePostRequest(
    string? Caption,
    PostVisibility? Visibility,
    string? Location,
    DateTime? ScheduledFor,
    PostStatus? Status);

public record PostDto(
    Guid Id,
    UserSummaryDto Author,
    string? Caption,
    IReadOnlyList<MediaItem> Media,
    string? Location,
    PostVisibility Visibility,
    PostStatus Status,
    int LikeCount,
    int CommentCount,
    int SaveCount,
    int ViewCount,
    string[]? AiTags,
    DateTime CreatedAt,
    DateTime UpdatedAt);
