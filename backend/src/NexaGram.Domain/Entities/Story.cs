namespace NexaGram.Domain.Entities;

public class Story
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string MediaUrl { get; set; } = "";
    public string MediaType { get; set; } = "image";
    public int DurationSeconds { get; set; } = 5;
    public DateTime ExpiresAt { get; set; }
    public int ViewCount { get; set; }
    public string? Layers { get; set; } // JSON
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StoryView> Views { get; set; } = [];
}
