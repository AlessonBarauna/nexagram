using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Engagement;

public class EngagementService : IEngagementService
{
    private readonly AppDbContext _db;

    public EngagementService(AppDbContext db) => _db = db;

    // ── Likes ──────────────────────────────────────────────────────────────

    public async Task LikePostAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var exists = await _db.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId, ct);
        if (exists) return;

        _db.Likes.Add(new Like { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
        await _db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, p => p.LikeCount + 1), ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UnlikePostAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var like = await _db.Likes.SingleOrDefaultAsync(l => l.PostId == postId && l.UserId == userId, ct);
        if (like is null) return;

        _db.Likes.Remove(like);
        await _db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, p => p.LikeCount - 1), ct);
        await _db.SaveChangesAsync(ct);
    }

    // ── Saves ──────────────────────────────────────────────────────────────

    public async Task SavePostAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var exists = await _db.Saves.AnyAsync(s => s.PostId == postId && s.UserId == userId, ct);
        if (exists) return;

        _db.Saves.Add(new Save { PostId = postId, UserId = userId, CreatedAt = DateTime.UtcNow });
        await _db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.SaveCount, p => p.SaveCount + 1), ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UnsavePostAsync(Guid postId, Guid userId, CancellationToken ct = default)
    {
        var save = await _db.Saves.SingleOrDefaultAsync(s => s.PostId == postId && s.UserId == userId, ct);
        if (save is null) return;

        _db.Saves.Remove(save);
        await _db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.SaveCount, p => p.SaveCount - 1), ct);
        await _db.SaveChangesAsync(ct);
    }

    // ── Comments ───────────────────────────────────────────────────────────

    public async Task<PagedResult<CommentDto>> GetCommentsAsync(
        Guid postId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Replies).ThenInclude(r => r.User)
            .Where(c => c.PostId == postId && c.ParentId == null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CommentDto>(items.Select(ToDto).ToList(), page, pageSize, total);
    }

    public async Task<CommentDto> CreateCommentAsync(
        Guid postId, Guid userId, CreateCommentRequest request, CancellationToken ct = default)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await _db.Comments.FindAsync([request.ParentId.Value], ct)
                ?? throw new KeyNotFoundException("Parent comment not found.");
            if (parent.PostId != postId)
                throw new InvalidOperationException("Parent comment does not belong to this post.");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            Content = request.Content,
            ParentId = request.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.Posts
            .Where(p => p.Id == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, p => p.CommentCount + 1), ct);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(comment).Reference(c => c.User).LoadAsync(ct);
        return ToDto(comment);
    }

    public async Task<CommentDto> UpdateCommentAsync(
        Guid commentId, Guid userId, UpdateCommentRequest request, CancellationToken ct = default)
    {
        var comment = await _db.Comments
            .Include(c => c.User)
            .Include(c => c.Replies).ThenInclude(r => r.User)
            .SingleOrDefaultAsync(c => c.Id == commentId, ct)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this comment.");

        comment.Content = request.Content;
        await _db.SaveChangesAsync(ct);
        return ToDto(comment);
    }

    public async Task DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default)
    {
        var comment = await _db.Comments
            .SingleOrDefaultAsync(c => c.Id == commentId, ct)
            ?? throw new KeyNotFoundException("Comment not found.");

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this comment.");

        var replyCount = await _db.Comments.CountAsync(c => c.ParentId == commentId, ct);
        var totalRemoved = 1 + replyCount;

        _db.Comments.Remove(comment);
        await _db.Posts
            .Where(p => p.Id == comment.PostId)
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.CommentCount, p => p.CommentCount - totalRemoved), ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task LikeCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default)
    {
        await _db.Comments
            .Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount + 1), ct);
    }

    public async Task UnlikeCommentAsync(Guid commentId, Guid userId, CancellationToken ct = default)
    {
        await _db.Comments
            .Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.LikeCount, c => c.LikeCount - 1), ct);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private static CommentDto ToDto(Comment c) => new(
        c.Id,
        new UserSummaryDto(c.User.Id, c.User.UserName!, c.User.DisplayName, c.User.AvatarUrl, c.User.IsVerified),
        c.Content,
        c.LikeCount,
        c.IsPinned,
        c.ParentId,
        c.Replies.Select(ToDto).ToList(),
        c.CreatedAt);
}
