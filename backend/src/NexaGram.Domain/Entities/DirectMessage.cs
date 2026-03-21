namespace NexaGram.Domain.Entities;

public class DirectMessage
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;
    public Guid ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public Guid? SharedPostId { get; set; }
    public Post? SharedPost { get; set; }
    public bool IsRead { get; set; }
    public bool IsEphemeral { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
