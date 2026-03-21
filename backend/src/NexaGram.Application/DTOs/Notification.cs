using NexaGram.Domain.Entities;

namespace NexaGram.Application.DTOs;

public record NotificationDto(
    Guid Id,
    NotificationType Type,
    UserSummaryDto Actor,
    Guid? EntityId,
    string? EntityType,
    bool IsRead,
    DateTime CreatedAt);
