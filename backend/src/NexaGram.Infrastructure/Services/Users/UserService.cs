using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Users;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;

    public UserService(UserManager<User> userManager, AppDbContext db, IStorageService storage)
    {
        _userManager = userManager;
        _db = db;
        _storage = storage;
    }

    public async Task<UserProfileDto> GetProfileAsync(string username, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.UserName == username, ct)
            ?? throw new KeyNotFoundException($"User '{username}' not found.");

        return ToProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        if (request.DisplayName is not null) user.DisplayName = request.DisplayName;
        if (request.Bio is not null) user.Bio = request.Bio;
        if (request.Website is not null) user.Website = request.Website;
        if (request.IsPrivate is not null) user.IsPrivate = request.IsPrivate.Value;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return ToProfileDto(user);
    }

    public async Task<PagedResult<UserSummaryDto>> GetFollowersAsync(string username, int page, int pageSize, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == username, ct)
            ?? throw new KeyNotFoundException($"User '{username}' not found.");

        var query = _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowingId == user.Id)
            .Select(f => f.Follower);

        return await ToPagedResultAsync(query, page, pageSize, ct);
    }

    public async Task<PagedResult<UserSummaryDto>> GetFollowingAsync(string username, int page, int pageSize, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == username, ct)
            ?? throw new KeyNotFoundException($"User '{username}' not found.");

        var query = _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == user.Id)
            .Select(f => f.Following);

        return await ToPagedResultAsync(query, page, pageSize, ct);
    }

    // --- private helpers ---

    private static async Task<PagedResult<UserSummaryDto>> ToPagedResultAsync(
        IQueryable<User> query, int page, int pageSize, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummaryDto(u.Id, u.UserName!, u.DisplayName, u.AvatarUrl, u.IsVerified))
            .ToListAsync(ct);

        return new PagedResult<UserSummaryDto>(items, page, pageSize, total);
    }

    public async Task<UserProfileDto> UpdateAvatarAsync(Guid userId, byte[] data, string mimeType, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        var key = $"avatars/{userId}/{Guid.NewGuid()}";
        var url = await _storage.UploadAsync(key, data, mimeType, ct);

        if (user.AvatarUrl is not null)
        {
            var oldKey = user.AvatarUrl.Split($"/").Last();
            try { await _storage.DeleteAsync($"avatars/{userId}/{oldKey}", ct); } catch { /* best-effort */ }
        }

        user.AvatarUrl = url;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return ToProfileDto(user);
    }

    private static UserProfileDto ToProfileDto(User user) =>
        new(user.Id, user.UserName!, user.DisplayName, user.Bio, user.AvatarUrl,
            user.Website, user.IsVerified, user.IsPrivate,
            user.FollowerCount, user.FollowingCount, user.PostCount, user.CreatedAt);
}
