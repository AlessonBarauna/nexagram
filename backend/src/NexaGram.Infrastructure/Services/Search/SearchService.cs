using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Search;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public SearchService(AppDbContext db) => _db = db;

    public async Task<SearchResultDto> SearchAsync(string query, int limit, CancellationToken ct = default)
    {
        var term = query.Trim().ToLower();

        var users = await _db.Users
            .AsNoTracking()
            .Where(u => u.UserName!.ToLower().Contains(term) || (u.DisplayName != null && u.DisplayName.ToLower().Contains(term)))
            .OrderByDescending(u => u.FollowerCount)
            .Take(limit)
            .Select(u => new UserSummaryDto(u.Id, u.UserName!, u.DisplayName, u.AvatarUrl, u.IsVerified))
            .ToListAsync(ct);

        var hashtags = await _db.Hashtags
            .AsNoTracking()
            .Where(h => h.Name.Contains(term.TrimStart('#')))
            .OrderByDescending(h => h.PostCount)
            .Take(limit)
            .Select(h => new HashtagDto(h.Id, h.Name, h.PostCount))
            .ToListAsync(ct);

        var posts = await _db.Posts
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.Status == PostStatus.Published &&
                        p.Visibility == PostVisibility.Public &&
                        p.Caption != null && p.Caption.ToLower().Contains(term))
            .OrderByDescending(p => p.LikeCount)
            .Take(limit)
            .ToListAsync(ct);

        return new SearchResultDto(users, hashtags, posts.Select(ToPostDto).ToList());
    }

    public async Task<PagedResult<PostDto>> GetPostsByHashtagAsync(
        string hashtag, int page, int pageSize, CancellationToken ct = default)
    {
        var name = hashtag.TrimStart('#').ToLower();

        var query = _db.PostHashtags
            .AsNoTracking()
            .Include(ph => ph.Post).ThenInclude(p => p.User)
            .Where(ph => ph.Hashtag.Name == name &&
                         ph.Post.Status == PostStatus.Published &&
                         ph.Post.Visibility == PostVisibility.Public)
            .Select(ph => ph.Post);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<PostDto>(items.Select(ToPostDto).ToList(), page, pageSize, total);
    }

    public async Task<IReadOnlyList<HashtagDto>> GetTrendingHashtagsAsync(int limit, CancellationToken ct = default)
    {
        return await _db.Hashtags
            .AsNoTracking()
            .OrderByDescending(h => h.PostCount)
            .Take(limit)
            .Select(h => new HashtagDto(h.Id, h.Name, h.PostCount))
            .ToListAsync(ct);
    }

    public async Task SyncPostHashtagsAsync(Guid postId, string? caption, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(caption)) return;

        var tagNames = Regex.Matches(caption, @"#(\w+)")
            .Select(m => m.Groups[1].Value.ToLower())
            .Distinct()
            .ToList();

        if (tagNames.Count == 0) return;

        // remove old associations
        var oldLinks = await _db.PostHashtags.Where(ph => ph.PostId == postId).ToListAsync(ct);
        if (oldLinks.Count > 0)
        {
            _db.PostHashtags.RemoveRange(oldLinks);
            // decrement counts for removed tags
            var oldTagIds = oldLinks.Select(l => l.HashtagId).ToList();
            await _db.Hashtags.Where(h => oldTagIds.Contains(h.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(h => h.PostCount, h => h.PostCount - 1), ct);
        }

        foreach (var name in tagNames)
        {
            var tag = await _db.Hashtags.SingleOrDefaultAsync(h => h.Name == name, ct);
            if (tag is null)
            {
                tag = new Hashtag { Id = Guid.NewGuid(), Name = name, PostCount = 0 };
                _db.Hashtags.Add(tag);
                await _db.SaveChangesAsync(ct);
            }

            _db.PostHashtags.Add(new PostHashtag { PostId = postId, HashtagId = tag.Id });
            tag.PostCount++;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static PostDto ToPostDto(Post p)
    {
        var media = string.IsNullOrEmpty(p.Media) || p.Media == "[]"
            ? (IReadOnlyList<MediaItem>)[]
            : JsonSerializer.Deserialize<List<MediaItem>>(p.Media, JsonOpts) ?? [];

        var aiTags = string.IsNullOrEmpty(p.AiTags)
            ? null
            : JsonSerializer.Deserialize<string[]>(p.AiTags, JsonOpts);

        var author = new UserSummaryDto(p.User.Id, p.User.UserName!, p.User.DisplayName, p.User.AvatarUrl, p.User.IsVerified);
        return new PostDto(p.Id, author, p.Caption, media, p.Location, p.Visibility, p.Status,
            p.LikeCount, p.CommentCount, p.SaveCount, p.ViewCount, aiTags, p.CreatedAt, p.UpdatedAt);
    }
}
