using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NexaGram.API.Hubs;

[Authorize]
public class CollabHub : Hub
{
    public async Task JoinSession(string sessionId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, $"collab:{sessionId}");

    public async Task UpdateCanvas(string sessionId, object delta) =>
        await Clients.OthersInGroup($"collab:{sessionId}").SendAsync("CanvasUpdated", delta, Context.UserIdentifier);

    public async Task SendChatMessage(string sessionId, string content) =>
        await Clients.Group($"collab:{sessionId}").SendAsync("ChatMessage", Context.UserIdentifier, content);
}
