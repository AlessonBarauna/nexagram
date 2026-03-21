namespace NexaGram.Application.DTOs;

public record CreateStoryRequest(
    string MediaKey,
    string MediaType = "image",
    int DurationSeconds = 5,
    int ExpiresInHours = 24,
    string? Layers = null);

public record StoryDto(
    Guid Id,
    UserSummaryDto Author,
    string MediaUrl,
    string MediaType,
    int DurationSeconds,
    DateTime ExpiresAt,
    int ViewCount,
    bool HasViewed,
    DateTime CreatedAt);

public record StoryFeedItemDto(
    UserSummaryDto User,
    IReadOnlyList<StoryDto> Stories,
    bool HasUnviewed);

public record StoryViewerDto(
    UserSummaryDto User,
    DateTime ViewedAt);
