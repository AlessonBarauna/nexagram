using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NexaGram.API.Hubs;

[Authorize]
public class NotificationHub : Hub { }
