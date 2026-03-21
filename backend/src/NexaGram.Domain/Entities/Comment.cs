namespace NexaGram.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public Comment? Parent { get; set; }
    public string Content { get; set; } = "";
    public int LikeCount { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Comment> Replies { get; set; } = [];
}
