using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Stories;

public class StoryService : IStoryService
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;

    public StoryService(AppDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<StoryDto> CreateAsync(Guid userId, CreateStoryRequest request, CancellationToken ct = default)
    {
        var story = new Story
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MediaUrl = _storage.GetPublicUrl(request.MediaKey),
            MediaType = request.MediaType,
            DurationSeconds = Math.Clamp(request.DurationSeconds, 1, 60),
            ExpiresAt = DateTime.UtcNow.AddHours(Math.Clamp(request.ExpiresInHours, 1, 24)),
            Layers = request.Layers,
            CreatedAt = DateTime.UtcNow
        };

        _db.Stories.Add(story);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(story).Reference(s => s.User).LoadAsync(ct);

        return ToDto(story, false);
    }

    public async Task<StoryDto> GetByIdAsync(Guid storyId, Guid? requestingUserId, CancellationToken ct = default)
    {
        var story = await _db.Stories
            .AsNoTracking()
            .Include(s => s.User)
            .SingleOrDefaultAsync(s => s.Id == storyId && s.ExpiresAt > DateTime.UtcNow, ct)
            ?? throw new KeyNotFoundException("Story not found or expired.");

        var hasViewed = requestingUserId.HasValue &&
            await _db.StoryViews.AnyAsync(v => v.StoryId == storyId && v.UserId == requestingUserId, ct);

        // auto-view if authenticated
        if (requestingUserId.HasValue && !hasViewed && requestingUserId != story.UserId)
            await ViewAsync(storyId, requestingUserId.Value, ct);

        return ToDto(story, hasViewed);
    }

    public async Task DeleteAsync(Guid storyId, Guid userId, CancellationToken ct = default)
    {
        var story = await _db.Stories.SingleOrDefaultAsync(s => s.Id == storyId, ct)
            ?? throw new KeyNotFoundException("Story not found.");

        if (story.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this story.");

        _db.Stories.Remove(story);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StoryFeedItemDto>> GetFeedAsync(Guid userId, CancellationToken ct = default)
    {
        var followingIds = await _db.Follows
            .AsNoTracking()
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync(ct);

        var viewedStoryIds = (await _db.StoryViews
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .Select(v => v.StoryId)
            .ToListAsync(ct)).ToHashSet();

        var stories = await _db.Stories
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => followingIds.Contains(s.UserId) && s.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        return stories
            .GroupBy(s => s.User)
            .Select(g =>
            {
                var storyDtos = g.Select(s => ToDto(s, viewedStoryIds.Contains(s.Id))).ToList();
                var author = new UserSummaryDto(g.Key.Id, g.Key.UserName!, g.Key.DisplayName, g.Key.AvatarUrl, g.Key.IsVerified);
                return new StoryFeedItemDto(author, storyDtos, storyDtos.Any(s => !s.HasViewed));
            })
            // unviewed users first
            .OrderByDescending(f => f.HasUnviewed)
            .ToList();
    }

    public async Task<IReadOnlyList<StoryDto>> GetUserStoriesAsync(
        string username, Guid? requestingUserId, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == username, ct)
            ?? throw new KeyNotFoundException($"User '{username}' not found.");

        var viewedIds = requestingUserId.HasValue
            ? (await _db.StoryViews.AsNoTracking()
                .Where(v => v.UserId == requestingUserId)
                .Select(v => v.StoryId)
                .ToListAsync(ct)).ToHashSet()
            : new HashSet<Guid>();

        var stories = await _db.Stories
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.UserId == user.Id && s.ExpiresAt > DateTime.UtcNow)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

        return stories.Select(s => ToDto(s, viewedIds.Contains(s.Id))).ToList();
    }

    public async Task ViewAsync(Guid storyId, Guid userId, CancellationToken ct = default)
    {
        var alreadyViewed = await _db.StoryViews
            .AnyAsync(v => v.StoryId == storyId && v.UserId == userId, ct);

        if (alreadyViewed) return;

        _db.StoryViews.Add(new StoryView
        {
            Id = Guid.NewGuid(),
            StoryId = storyId,
            UserId = userId,
            ViewedAt = DateTime.UtcNow
        });

        await _db.Stories
            .Where(s => s.Id == storyId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), ct);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<StoryViewerDto>> GetViewersAsync(
        Guid storyId, Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var story = await _db.Stories.AsNoTracking().SingleOrDefaultAsync(s => s.Id == storyId, ct)
            ?? throw new KeyNotFoundException("Story not found.");

        if (story.UserId != userId)
            throw new UnauthorizedAccessException("Only the story owner can see viewers.");

        var query = _db.StoryViews
            .AsNoTracking()
            .Include(v => v.User)
            .Where(v => v.StoryId == storyId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new StoryViewerDto(
                new UserSummaryDto(v.User.Id, v.User.UserName!, v.User.DisplayName, v.User.AvatarUrl, v.User.IsVerified),
                v.ViewedAt))
            .ToListAsync(ct);

        return new PagedResult<StoryViewerDto>(items, page, pageSize, total);
    }

    private static StoryDto ToDto(Story s, bool hasViewed) => new(
        s.Id,
        new UserSummaryDto(s.User.Id, s.User.UserName!, s.User.DisplayName, s.User.AvatarUrl, s.User.IsVerified),
        s.MediaUrl, s.MediaType, s.DurationSeconds, s.ExpiresAt, s.ViewCount, hasViewed, s.CreatedAt);
}
