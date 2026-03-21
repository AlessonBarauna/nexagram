using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Social;

public class FollowService : IFollowService
{
    private readonly AppDbContext _db;

    public FollowService(AppDbContext db) => _db = db;

    public async Task FollowAsync(Guid followerId, Guid followingId, CancellationToken ct = default)
    {
        if (followerId == followingId)
            throw new InvalidOperationException("You cannot follow yourself.");

        var exists = await _db.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, ct);
        if (exists) return;

        _db.Follows.Add(new Follow { FollowerId = followerId, FollowingId = followingId, CreatedAt = DateTime.UtcNow });

        await _db.Users.Where(u => u.Id == followerId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowingCount, u => u.FollowingCount + 1), ct);
        await _db.Users.Where(u => u.Id == followingId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowerCount, u => u.FollowerCount + 1), ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task UnfollowAsync(Guid followerId, Guid followingId, CancellationToken ct = default)
    {
        var follow = await _db.Follows
            .SingleOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, ct);
        if (follow is null) return;

        _db.Follows.Remove(follow);

        await _db.Users.Where(u => u.Id == followerId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowingCount, u => u.FollowingCount - 1), ct);
        await _db.Users.Where(u => u.Id == followingId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.FollowerCount, u => u.FollowerCount - 1), ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId, CancellationToken ct = default) =>
        await _db.Follows.AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, ct);

    public async Task<PagedResult<UserSummaryDto>> GetSuggestionsAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        // Users followed by people I follow (friends-of-friends), excluding already followed + self
        var myFollowingIds = await _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync(ct);

        var suggested = _db.Follows
            .AsNoTracking()
            .Where(f => myFollowingIds.Contains(f.FollowerId) &&
                        f.FollowingId != userId &&
                        !myFollowingIds.Contains(f.FollowingId))
            .Select(f => f.Following)
            .Distinct();

        var total = await suggested.CountAsync(ct);

        // fallback: if not enough suggestions, fill with popular accounts
        if (total < pageSize)
        {
            var fallback = _db.Users
                .AsNoTracking()
                .Where(u => u.Id != userId && !myFollowingIds.Contains(u.Id))
                .OrderByDescending(u => u.FollowerCount);

            var combined = suggested.Union(fallback);
            total = await combined.CountAsync(ct);

            var items = await combined
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserSummaryDto(u.Id, u.UserName!, u.DisplayName, u.AvatarUrl, u.IsVerified))
                .ToListAsync(ct);

            return new PagedResult<UserSummaryDto>(items, page, pageSize, total);
        }

        var result = await suggested
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummaryDto(u.Id, u.UserName!, u.DisplayName, u.AvatarUrl, u.IsVerified))
            .ToListAsync(ct);

        return new PagedResult<UserSummaryDto>(result, page, pageSize, total);
    }
}
