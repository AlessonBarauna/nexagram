using Microsoft.AspNetCore.SignalR;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;
using NexaGram.API.Hubs;

namespace NexaGram.API.Services;

public class SignalRChatNotifier : IChatNotifier
{
    private readonly IHubContext<ChatHub> _hub;

    public SignalRChatNotifier(IHubContext<ChatHub> hub) => _hub = hub;

    public Task SendMessageAsync(Guid recipientId, DirectMessageDto message, CancellationToken ct = default) =>
        _hub.Clients.User(recipientId.ToString()).SendAsync("newMessage", message, ct);
}
