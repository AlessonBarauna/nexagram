namespace NexaGram.Domain.Entities;

public class StoryView
{
    public Guid Id { get; set; }
    public Guid StoryId { get; set; }
    public Story Story { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}
