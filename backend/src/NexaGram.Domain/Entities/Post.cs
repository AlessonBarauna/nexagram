using Pgvector;

namespace NexaGram.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string? Caption { get; set; }
    public string Media { get; set; } = "[]"; // JSON array
    public string? Location { get; set; } // JSON: {lat, lng, name}
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    public PostStatus Status { get; set; } = PostStatus.Published;
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int SaveCount { get; set; }
    public int ViewCount { get; set; }
    public string? AiTags { get; set; } // JSON array
    public string? AiDescription { get; set; }
    public Vector? Embedding { get; set; } // pgvector(384)
    public DateTime? ScheduledFor { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Save> Saves { get; set; } = [];
    public ICollection<PostHashtag> PostHashtags { get; set; } = [];
}

public enum PostVisibility { Public, Followers, CloseFriends, Private }
public enum PostStatus { Draft, Scheduled, Published, Deleted }
