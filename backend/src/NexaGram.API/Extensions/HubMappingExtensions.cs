using NexaGram.API.Hubs;

namespace NexaGram.API.Extensions;

public static class HubMappingExtensions
{
    public static WebApplication MapHubs(this WebApplication app)
    {
        app.MapHub<FeedHub>("/hubs/feed");
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapHub<ChatHub>("/hubs/chat");
        app.MapHub<CollabHub>("/hubs/collab");
        app.MapHub<WorldHub>("/hubs/world");
        return app;
    }
}
