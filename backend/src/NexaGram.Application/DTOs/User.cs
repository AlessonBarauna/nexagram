namespace NexaGram.Application.DTOs;

public record UserProfileDto(
    Guid Id,
    string Username,
    string? DisplayName,
    string? Bio,
    string? AvatarUrl,
    string? Website,
    bool IsVerified,
    bool IsPrivate,
    int FollowerCount,
    int FollowingCount,
    int PostCount,
    DateTime CreatedAt);

public record UserSummaryDto(
    Guid Id,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    bool IsVerified);

public record UpdateProfileRequest(
    string? DisplayName,
    string? Bio,
    string? Website,
    bool? IsPrivate);
