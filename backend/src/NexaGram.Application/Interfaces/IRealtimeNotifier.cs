using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IRealtimeNotifier
{
    Task SendNotificationAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);
}
