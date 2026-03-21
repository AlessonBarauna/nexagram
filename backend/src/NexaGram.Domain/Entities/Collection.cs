namespace NexaGram.Domain.Entities;

public class Collection
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = "";
    public string? CoverUrl { get; set; }
    public int PostCount { get; set; }
    public bool IsPrivate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Save> Saves { get; set; } = [];
}
