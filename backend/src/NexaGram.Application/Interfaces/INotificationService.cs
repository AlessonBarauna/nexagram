using NexaGram.Application.DTOs;
using NexaGram.Domain.Entities;

namespace NexaGram.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
    Task PushAsync(Guid recipientId, Guid actorId, NotificationType type, Guid? entityId = null, string? entityType = null, CancellationToken ct = default);
}
