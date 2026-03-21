using Microsoft.EntityFrameworkCore;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.Domain.Entities;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IRealtimeNotifier _notifier;

    public NotificationService(AppDbContext db, IRealtimeNotifier notifier)
    {
        _db = db;
        _notifier = notifier;
    }

    public async Task<PagedResult<NotificationDto>> GetAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Include(n => n.Actor)
            .Where(n => n.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => ToDto(n))
            .ToListAsync(ct);

        return new PagedResult<NotificationDto>(items, page, pageSize, total);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) =>
        await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        await _db.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
    }

    public async Task PushAsync(
        Guid recipientId, Guid actorId, NotificationType type,
        Guid? entityId = null, string? entityType = null, CancellationToken ct = default)
    {
        // don't notify yourself
        if (recipientId == actorId) return;

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = recipientId,
            ActorId = actorId,
            Type = type,
            EntityId = entityId,
            EntityType = entityType,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);

        // load actor for real-time push
        var actor = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == actorId, ct);
        if (actor is null) return;

        notification.Actor = actor;
        var dto = ToDto(notification);

        await _notifier.SendNotificationAsync(recipientId, dto, ct);
    }

    private static NotificationDto ToDto(Notification n) => new(
        n.Id, n.Type,
        new UserSummaryDto(n.Actor.Id, n.Actor.UserName!, n.Actor.DisplayName, n.Actor.AvatarUrl, n.Actor.IsVerified),
        n.EntityId, n.EntityType, n.IsRead, n.CreatedAt);
}
