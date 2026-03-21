using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(string username, CancellationToken ct = default);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<PagedResult<UserSummaryDto>> GetFollowersAsync(string username, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<UserSummaryDto>> GetFollowingAsync(string username, int page, int pageSize, CancellationToken ct = default);
    Task<UserProfileDto> UpdateAvatarAsync(Guid userId, byte[] data, string mimeType, CancellationToken ct = default);
}
