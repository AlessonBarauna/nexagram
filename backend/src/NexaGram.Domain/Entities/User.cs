using Microsoft.AspNetCore.Identity;

namespace NexaGram.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Website { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPrivate { get; set; }
    public int FollowerCount { get; set; }
    public int FollowingCount { get; set; }
    public int PostCount { get; set; }
    public string? Settings { get; set; } // JSON: feed_weights, notification_prefs, privacy_config
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Post> Posts { get; set; } = [];
    public ICollection<Story> Stories { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Save> Saves { get; set; } = [];
    public ICollection<Collection> Collections { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<DirectMessage> SentMessages { get; set; } = [];
    public ICollection<DirectMessage> ReceivedMessages { get; set; } = [];
}

public class AppRole : IdentityRole<Guid>
{
    public AppRole() : base() { }
    public AppRole(string roleName) : base(roleName) { }
}
