using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NexaGram.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinConversation(string userId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat:{userId}");

    public async Task StartTyping(string toUserId) =>
        await Clients.Group($"chat:{toUserId}").SendAsync("UserTyping", Context.UserIdentifier);

    public async Task StopTyping(string toUserId) =>
        await Clients.Group($"chat:{toUserId}").SendAsync("UserStoppedTyping", Context.UserIdentifier);
}
