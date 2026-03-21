using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Collections;

public class CollectionService : ICollectionService
{
    private readonly AppDbContext _db;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CollectionService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CollectionDto>> GetUserCollectionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Collections
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CollectionDto(c.Id, c.Name, c.CoverUrl, c.PostCount, c.IsPrivate, c.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<PagedResult<PostDto>> GetCollectionPostsAsync(
        Guid collectionId, Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var collection = await _db.Collections.AsNoTracking().SingleOrDefaultAsync(c => c.Id == collectionId, ct)
            ?? throw new KeyNotFoundException("Collection not found.");

        if (collection.UserId != userId && collection.IsPrivate)
            throw new UnauthorizedAccessException("This collection is private.");

        var query = _db.Saves
            .AsNoTracking()
            .Include(s => s.Post).ThenInclude(p => p.User)
            .Where(s => s.CollectionId == collectionId)
            .Select(s => s.Post);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PostDto>(items.Select(ToPostDto).ToList(), page, pageSize, total);
    }

    public async Task<CollectionDto> CreateAsync(Guid userId, CreateCollectionRequest request, CancellationToken ct = default)
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            IsPrivate = request.IsPrivate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Collections.Add(collection);
        await _db.SaveChangesAsync(ct);

        return new CollectionDto(collection.Id, collection.Name, collection.CoverUrl, 0, collection.IsPrivate, collection.CreatedAt);
    }

    public async Task<CollectionDto> UpdateAsync(Guid collectionId, Guid userId, UpdateCollectionRequest request, CancellationToken ct = default)
    {
        var collection = await _db.Collections.SingleOrDefaultAsync(c => c.Id == collectionId, ct)
            ?? throw new KeyNotFoundException("Collection not found.");

        if (collection.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this collection.");

        if (request.Name is not null) collection.Name = request.Name;
        if (request.IsPrivate is not null) collection.IsPrivate = request.IsPrivate.Value;

        await _db.SaveChangesAsync(ct);
        return new CollectionDto(collection.Id, collection.Name, collection.CoverUrl, collection.PostCount, collection.IsPrivate, collection.CreatedAt);
    }

    public async Task DeleteAsync(Guid collectionId, Guid userId, CancellationToken ct = default)
    {
        var collection = await _db.Collections.SingleOrDefaultAsync(c => c.Id == collectionId, ct)
            ?? throw new KeyNotFoundException("Collection not found.");

        if (collection.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this collection.");

        _db.Collections.Remove(collection);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddPostAsync(Guid collectionId, Guid postId, Guid userId, CancellationToken ct = default)
    {
        var collection = await _db.Collections.SingleOrDefaultAsync(c => c.Id == collectionId, ct)
            ?? throw new KeyNotFoundException("Collection not found.");

        if (collection.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this collection.");

        var exists = await _db.Saves.AnyAsync(s => s.CollectionId == collectionId && s.PostId == postId, ct);
        if (exists) return;

        // ensure the post is also saved (Save = save + optional collection)
        var saved = await _db.Saves.SingleOrDefaultAsync(s => s.UserId == userId && s.PostId == postId, ct);
        if (saved is null)
        {
            _db.Saves.Add(new Save { UserId = userId, PostId = postId, CollectionId = collectionId, CreatedAt = DateTime.UtcNow });
            await _db.Posts.Where(p => p.Id == postId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.SaveCount, p => p.SaveCount + 1), ct);
        }
        else
        {
            saved.CollectionId = collectionId;
        }

        collection.PostCount++;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemovePostAsync(Guid collectionId, Guid postId, Guid userId, CancellationToken ct = default)
    {
        var collection = await _db.Collections.SingleOrDefaultAsync(c => c.Id == collectionId, ct)
            ?? throw new KeyNotFoundException("Collection not found.");

        if (collection.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this collection.");

        var save = await _db.Saves.SingleOrDefaultAsync(s => s.CollectionId == collectionId && s.PostId == postId, ct);
        if (save is null) return;

        save.CollectionId = null;
        collection.PostCount = Math.Max(0, collection.PostCount - 1);
        await _db.SaveChangesAsync(ct);
    }

    private static PostDto ToPostDto(Post p)
    {
        var media = string.IsNullOrEmpty(p.Media) || p.Media == "[]"
            ? (IReadOnlyList<MediaItem>)[]
            : JsonSerializer.Deserialize<List<MediaItem>>(p.Media, JsonOpts) ?? [];

        var author = new UserSummaryDto(p.User.Id, p.User.UserName!, p.User.DisplayName, p.User.AvatarUrl, p.User.IsVerified);
        return new PostDto(p.Id, author, p.Caption, media, p.Location, p.Visibility, p.Status,
            p.LikeCount, p.CommentCount, p.SaveCount, p.ViewCount, null, p.CreatedAt, p.UpdatedAt);
    }
}
