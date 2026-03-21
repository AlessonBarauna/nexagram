namespace NexaGram.Domain.Entities;

public class CollabSession
{
    public Guid Id { get; set; }
    public Guid CreatorId { get; set; }
    public User Creator { get; set; } = null!;
    public string Title { get; set; } = "";
    public CollabStatus Status { get; set; } = CollabStatus.Active;
    public string? CanvasState { get; set; } // JSON
    public string ParticipantIds { get; set; } = "[]"; // JSON array of Guid
    public Guid? ResultPostId { get; set; }
    public Post? ResultPost { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum CollabStatus { Active, Closed }
