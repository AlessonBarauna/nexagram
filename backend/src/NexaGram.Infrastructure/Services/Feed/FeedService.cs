using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Feed;

public class FeedService : IFeedService
{
    private readonly AppDbContext _db;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public FeedService(AppDbContext db) => _db = db;

    public async Task<PagedResult<PostDto>> GetPersonalFeedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        // get IDs of users the current user follows
        var followingIds = await _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync(ct);

        // include own posts too
        followingIds.Add(userId);

        var query = _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p =>
                followingIds.Contains(p.UserId) &&
                p.Status == PostStatus.Published &&
                (p.Visibility == PostVisibility.Public ||
                 p.Visibility == PostVisibility.Followers ||
                 p.Visibility == PostVisibility.CloseFriends ||
                 p.UserId == userId));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PostDto>(items.Select(ToDto).ToList(), page, pageSize, total);
    }

    public async Task<PagedResult<PostDto>> GetExploreFeedAsync(
        Guid? userId, int page, int pageSize, CancellationToken ct = default)
    {
        // exclude users already followed (if authenticated)
        IQueryable<Post> query = _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.Status == PostStatus.Published && p.Visibility == PostVisibility.Public);

        if (userId.HasValue)
        {
            var followingIds = await _db.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync(ct);

            // prefer posts from non-followed users on explore
            query = query.Where(p => !followingIds.Contains(p.UserId) && p.UserId != userId);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            // rank by engagement score
            .OrderByDescending(p => p.LikeCount + p.CommentCount * 2 + p.SaveCount * 3)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PostDto>(items.Select(ToDto).ToList(), page, pageSize, total);
    }

    private static PostDto ToDto(Post post)
    {
        var media = string.IsNullOrEmpty(post.Media) || post.Media == "[]"
            ? (IReadOnlyList<MediaItem>)[]
            : JsonSerializer.Deserialize<List<MediaItem>>(post.Media, JsonOpts) ?? [];

        var aiTags = string.IsNullOrEmpty(post.AiTags)
            ? null
            : JsonSerializer.Deserialize<string[]>(post.AiTags, JsonOpts);

        var author = new UserSummaryDto(
            post.User.Id, post.User.UserName!, post.User.DisplayName,
            post.User.AvatarUrl, post.User.IsVerified);

        return new PostDto(
            post.Id, author, post.Caption, media, post.Location,
            post.Visibility, post.Status,
            post.LikeCount, post.CommentCount, post.SaveCount, post.ViewCount,
            aiTags, post.CreatedAt, post.UpdatedAt);
    }
}
