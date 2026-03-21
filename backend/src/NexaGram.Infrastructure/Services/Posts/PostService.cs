using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Posts;

public class PostService : IPostService
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;
    private readonly ISearchService _search;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public PostService(AppDbContext db, IStorageService storage, ISearchService search)
    {
        _db = db;
        _storage = storage;
        _search = search;
    }

    public async Task<PostDto> CreateAsync(Guid userId, CreatePostRequest request, CancellationToken ct = default)
    {
        var mediaItems = request.Media
            .Select(m => new MediaItem(_storage.GetPublicUrl(m.Key), m.MimeType))
            .ToList();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Caption = request.Caption,
            Media = JsonSerializer.Serialize(mediaItems, JsonOpts),
            Visibility = request.Visibility,
            Status = request.ScheduledFor.HasValue ? PostStatus.Scheduled : request.Status,
            Location = request.Location,
            ScheduledFor = request.ScheduledFor,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Posts.Add(post);

        // increment user post count
        await _db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PostCount, u => u.PostCount + 1), ct);

        await _db.SaveChangesAsync(ct);

        await _db.Entry(post).Reference(p => p.User).LoadAsync(ct);

        if (post.Status == PostStatus.Published)
            await _search.SyncPostHashtagsAsync(post.Id, post.Caption, ct);

        return ToDto(post);
    }

    public async Task<PostDto> GetByIdAsync(Guid postId, Guid? requestingUserId, CancellationToken ct = default)
    {
        var post = await _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == postId, ct)
            ?? throw new KeyNotFoundException("Post not found.");

        if (post.Status == PostStatus.Deleted)
            throw new KeyNotFoundException("Post not found.");

        await CheckVisibilityAsync(post, requestingUserId, ct);

        return ToDto(post);
    }

    public async Task<PostDto> UpdateAsync(Guid postId, Guid userId, UpdatePostRequest request, CancellationToken ct = default)
    {
        var post = await _db.Posts
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == postId, ct)
            ?? throw new KeyNotFoundException("Post not found.");

        if (post.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this post.");

        if (request.Caption is not null) post.Caption = request.Caption;
        if (request.Visibility is not null) post.Visibility = request.Visibility.Value;
        if (request.Location is not null) post.Location = request.Location;
        if (request.ScheduledFor is not null) post.ScheduledFor = request.ScheduledFor;
        if (request.Status is not null) post.Status = request.Status.Value;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        if (post.Status == PostStatus.Published)
            await _search.SyncPostHashtagsAsync(post.Id, post.Caption, ct);

        return ToDto(post);
    }

    public async Task DeleteAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var post = await _db.Posts
            .SingleOrDefaultAsync(p => p.Id == postId, ct)
            ?? throw new KeyNotFoundException("Post not found.");

        if (post.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this post.");

        post.Status = PostStatus.Deleted;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.PostCount, u => u.PostCount - 1), ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<PostDto>> GetUserPostsAsync(
        string username, Guid? requestingUserId, int page, int pageSize, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == username, ct)
            ?? throw new KeyNotFoundException($"User '{username}' not found.");

        var isOwner = requestingUserId == user.Id;
        var isFollowing = !isOwner && requestingUserId.HasValue &&
            await _db.Follows.AnyAsync(f => f.FollowerId == requestingUserId && f.FollowingId == user.Id, ct);

        var query = _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.UserId == user.Id && p.Status != PostStatus.Deleted);

        if (!isOwner)
        {
            query = isFollowing
                ? query.Where(p => p.Visibility != PostVisibility.Private)
                : query.Where(p => p.Visibility == PostVisibility.Public);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PostDto>(items.Select(ToDto).ToList(), page, pageSize, total);
    }

    // --- private helpers ---

    private async Task CheckVisibilityAsync(Post post, Guid? requestingUserId, CancellationToken ct)
    {
        if (post.UserId == requestingUserId) return;

        if (post.Visibility == PostVisibility.Private)
            throw new UnauthorizedAccessException("This post is private.");

        if (post.Visibility is PostVisibility.Followers or PostVisibility.CloseFriends)
        {
            if (!requestingUserId.HasValue)
                throw new UnauthorizedAccessException("Authentication required to view this post.");

            var follows = await _db.Follows.AnyAsync(
                f => f.FollowerId == requestingUserId && f.FollowingId == post.UserId, ct);

            if (!follows)
                throw new UnauthorizedAccessException("You must follow this user to view this post.");
        }
    }

    private static PostDto ToDto(Post post)
    {
        var media = string.IsNullOrEmpty(post.Media) || post.Media == "[]"
            ? []
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
