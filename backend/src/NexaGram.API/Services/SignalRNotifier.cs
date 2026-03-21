using Microsoft.AspNetCore.SignalR;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.API.Hubs;

namespace NexaGram.API.Services;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotifier(IHubContext<NotificationHub> hub) => _hub = hub;

    public Task SendNotificationAsync(Guid userId, NotificationDto notification, CancellationToken ct = default) =>
        _hub.Clients.User(userId.ToString()).SendAsync("notification", notification, ct);
}
