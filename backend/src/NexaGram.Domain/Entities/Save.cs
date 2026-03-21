namespace NexaGram.Domain.Entities;

public class Save
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid? CollectionId { get; set; }
    public Collection? Collection { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
