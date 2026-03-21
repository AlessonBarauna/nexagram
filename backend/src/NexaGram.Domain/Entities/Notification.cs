namespace NexaGram.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid ActorId { get; set; }
    public User Actor { get; set; } = null!;
    public NotificationType Type { get; set; }
    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    Follow,
    Like,
    Comment,
    Mention,
    StoryView,
    CollabInvite
}
