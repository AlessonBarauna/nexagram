using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Chat;

public class DirectMessageService : IDirectMessageService
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;
    private readonly IChatNotifier _chat;

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public DirectMessageService(AppDbContext db, IStorageService storage, IChatNotifier chat)
    {
        _db = db;
        _storage = storage;
        _chat = chat;
    }

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default)
    {
        var messages = await _db.DirectMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) &&
                        (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        return messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var last = g.First();
                var participant = last.SenderId == userId ? last.Receiver : last.Sender;
                var unread = g.Count(m => m.ReceiverId == userId && !m.IsRead);
                return new ConversationDto(
                    ToUserSummary(participant),
                    ToDto(last),
                    unread);
            })
            .ToList();
    }

    public async Task<PagedResult<DirectMessageDto>> GetMessagesAsync(
        Guid userId, Guid participantId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.DirectMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.SharedPost).ThenInclude(p => p!.User)
            .Where(m =>
                ((m.SenderId == userId && m.ReceiverId == participantId) ||
                 (m.SenderId == participantId && m.ReceiverId == userId)) &&
                (m.ExpiresAt == null || m.ExpiresAt > DateTime.UtcNow));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // mark received messages as read
        await _db.DirectMessages
            .Where(m => m.SenderId == participantId && m.ReceiverId == userId && !m.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);

        return new PagedResult<DirectMessageDto>(items.Select(ToDto).ToList(), page, pageSize, total);
    }

    public async Task<DirectMessageDto> SendAsync(Guid senderId, SendMessageRequest request, CancellationToken ct = default)
    {
        var receiver = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == request.RecipientUsername, ct)
            ?? throw new KeyNotFoundException($"User '{request.RecipientUsername}' not found.");

        var message = new DirectMessage
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = receiver.Id,
            Content = request.Content,
            MediaUrl = request.MediaKey is not null ? _storage.GetPublicUrl(request.MediaKey) : null,
            SharedPostId = request.SharedPostId,
            IsEphemeral = request.IsEphemeral,
            ExpiresAt = request.IsEphemeral && request.EphemeralSeconds.HasValue
                ? DateTime.UtcNow.AddSeconds(request.EphemeralSeconds.Value)
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _db.DirectMessages.Add(message);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(message).Reference(m => m.Sender).LoadAsync(ct);
        await _db.Entry(message).Reference(m => m.Receiver).LoadAsync(ct);

        var dto = ToDto(message);

        // real-time push to recipient
        await _chat.SendMessageAsync(receiver.Id, dto, ct);

        return dto;
    }

    public async Task MarkReadAsync(Guid messageId, Guid userId, CancellationToken ct = default)
    {
        await _db.DirectMessages
            .Where(m => m.Id == messageId && m.ReceiverId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true), ct);
    }

    public async Task DeleteAsync(Guid messageId, Guid userId, CancellationToken ct = default)
    {
        var message = await _db.DirectMessages.SingleOrDefaultAsync(m => m.Id == messageId, ct)
            ?? throw new KeyNotFoundException("Message not found.");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You cannot delete this message.");

        _db.DirectMessages.Remove(message);
        await _db.SaveChangesAsync(ct);
    }

    private static UserSummaryDto ToUserSummary(User u) =>
        new(u.Id, u.UserName!, u.DisplayName, u.AvatarUrl, u.IsVerified);

    private DirectMessageDto ToDto(DirectMessage m)
    {
        PostDto? sharedPost = null;
        if (m.SharedPost is not null)
        {
            var media = string.IsNullOrEmpty(m.SharedPost.Media) || m.SharedPost.Media == "[]"
                ? (IReadOnlyList<MediaItem>)[]
                : JsonSerializer.Deserialize<List<MediaItem>>(m.SharedPost.Media, JsonOpts) ?? [];
            var author = ToUserSummary(m.SharedPost.User);
            sharedPost = new PostDto(m.SharedPost.Id, author, m.SharedPost.Caption, media,
                m.SharedPost.Location, m.SharedPost.Visibility, m.SharedPost.Status,
                m.SharedPost.LikeCount, m.SharedPost.CommentCount, m.SharedPost.SaveCount,
                m.SharedPost.ViewCount, null, m.SharedPost.CreatedAt, m.SharedPost.UpdatedAt);
        }

        return new DirectMessageDto(m.Id, ToUserSummary(m.Sender), ToUserSummary(m.Receiver),
            m.Content, m.MediaUrl, sharedPost, m.IsRead, m.IsEphemeral, m.ExpiresAt, m.CreatedAt);
    }
}
