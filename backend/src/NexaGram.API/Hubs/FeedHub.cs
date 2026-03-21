using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NexaGram.API.Hubs;

[Authorize]
public class FeedHub : Hub
{
    public async Task JoinFeed() => await Groups.AddToGroupAsync(Context.ConnectionId, "feed");
    public async Task LeaveFeed() => await Groups.RemoveFromGroupAsync(Context.ConnectionId, "feed");
}
