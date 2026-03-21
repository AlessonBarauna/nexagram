using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence;

public class DbSeeder
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext db, UserManager<User> userManager, ILogger<DbSeeder> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _db.Users.AnyAsync()) return;

        _logger.LogInformation("Seeding development data...");

        // Create seed users
        var users = new[]
        {
            new { Username = "nexagram_admin", Email = "admin@nexagram.dev", Name = "NexaGram Admin", Bio = "The official NexaGram account", Verified = true },
            new { Username = "alice_creates", Email = "alice@nexagram.dev", Name = "Alice Creates", Bio = "📸 Visual storyteller | 🌎 Travel & lifestyle", Verified = false },
            new { Username = "bob_dev", Email = "bob@nexagram.dev", Name = "Bob Dev", Bio = "💻 Code. Coffee. Repeat.", Verified = false },
            new { Username = "carla_photo", Email = "carla@nexagram.dev", Name = "Carla Photo", Bio = "🌿 Nature photographer | 📍 Brazil", Verified = true },
        };

        var createdUsers = new List<User>();

        foreach (var u in users)
        {
            var user = new User
            {
                UserName = u.Username,
                Email = u.Email,
                DisplayName = u.Name,
                Bio = u.Bio,
                IsVerified = u.Verified,
                EmailConfirmed = true,
                Settings = """{"feedWeights":{"friends":0.4,"interests":0.3,"discovery":0.2,"local":0.1},"notifications":{"likes":true,"comments":true,"follows":true},"privacy":{"isPrivate":false}}"""
            };

            var result = await _userManager.CreateAsync(user, "Password123!");
            if (result.Succeeded)
            {
                createdUsers.Add(user);
                _logger.LogInformation("Created seed user: {Username}", user.UserName);
            }
            else
            {
                _logger.LogWarning("Failed to create seed user {Username}: {Errors}",
                    u.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        if (createdUsers.Count < 2) return;

        // Create some follows
        var alice = createdUsers.FirstOrDefault(u => u.UserName == "alice_creates");
        var bob = createdUsers.FirstOrDefault(u => u.UserName == "bob_dev");
        var carla = createdUsers.FirstOrDefault(u => u.UserName == "carla_photo");
        var admin = createdUsers.FirstOrDefault(u => u.UserName == "nexagram_admin");

        if (alice != null && bob != null && carla != null && admin != null)
        {
            var follows = new[]
            {
                new Follow { FollowerId = alice.Id, FollowingId = bob.Id },
                new Follow { FollowerId = alice.Id, FollowingId = carla.Id },
                new Follow { FollowerId = bob.Id, FollowingId = alice.Id },
                new Follow { FollowerId = carla.Id, FollowingId = alice.Id },
                new Follow { FollowerId = admin.Id, FollowingId = alice.Id },
                new Follow { FollowerId = admin.Id, FollowingId = carla.Id },
            };
            _db.Follows.AddRange(follows);

            // Update follower/following counts
            alice.FollowerCount = 2; alice.FollowingCount = 2;
            bob.FollowerCount = 1; bob.FollowingCount = 1;
            carla.FollowerCount = 2; carla.FollowingCount = 1;
            admin.FollowerCount = 0; admin.FollowingCount = 2;

            // Create some posts
            var posts = new[]
            {
                new Post
                {
                    UserId = alice.Id,
                    Caption = "Golden hour magic ✨ The world looks different when the light hits just right. #photography #goldenhour #travel",
                    Media = """[{"url":"https://picsum.photos/seed/nexagram1/800/800","type":"image","width":800,"height":800,"blurhash":"L6PZfSi_.AyE_3t7t7R**0o#DgR4","altText":"Beautiful golden hour sunset over mountains"}]""",
                    Location = """{"lat":-23.5505,"lng":-46.6333,"name":"São Paulo, Brazil"}""",
                    Visibility = PostVisibility.Public,
                    Status = PostStatus.Published,
                    LikeCount = 42,
                    CommentCount = 5,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Post
                {
                    UserId = carla.Id,
                    Caption = "🌿 Found this hidden waterfall after 3h hiking. Worth every step. Nature never disappoints. #nature #hiking #waterfall",
                    Media = """[{"url":"https://picsum.photos/seed/nexagram2/800/1000","type":"image","width":800,"height":1000,"blurhash":"LKO2?U%2Tw=w]~RBVZRi};RPxuwH","altText":"Hidden waterfall surrounded by lush green vegetation"}]""",
                    Location = """{"lat":-15.7801,"lng":-47.9292,"name":"Brasília, Brazil"}""",
                    Visibility = PostVisibility.Public,
                    Status = PostStatus.Published,
                    LikeCount = 128,
                    CommentCount = 12,
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new Post
                {
                    UserId = bob.Id,
                    Caption = "Shipped v2.0 today 🚀 6 months of work, 47k lines of code, countless coffees. Building something you believe in hits different. #dev #coding #startup",
                    Media = """[{"url":"https://picsum.photos/seed/nexagram3/800/600","type":"image","width":800,"height":600,"blurhash":"LEHV6nWB2yk8pyoJadR*.7kCMdnj","altText":"Developer laptop with code on screen"}]""",
                    Visibility = PostVisibility.Public,
                    Status = PostStatus.Published,
                    LikeCount = 89,
                    CommentCount = 23,
                    CreatedAt = DateTime.UtcNow.AddHours(-8)
                },
            };
            _db.Posts.AddRange(posts);

            // Create hashtags
            var hashtags = new[]
            {
                new Hashtag { Name = "photography", PostCount = 1 },
                new Hashtag { Name = "goldenhour", PostCount = 1 },
                new Hashtag { Name = "travel", PostCount = 1 },
                new Hashtag { Name = "nature", PostCount = 1 },
                new Hashtag { Name = "hiking", PostCount = 1 },
                new Hashtag { Name = "waterfall", PostCount = 1 },
                new Hashtag { Name = "dev", PostCount = 1 },
                new Hashtag { Name = "coding", PostCount = 1 },
                new Hashtag { Name = "startup", PostCount = 1 },
            };
            _db.Hashtags.AddRange(hashtags);

            await _db.SaveChangesAsync();

            // Create PostHashtag links
            var alicePost = posts[0];
            var carlaPost = posts[1];
            var bobPost = posts[2];

            var hashtagDict = await _db.Hashtags.ToDictionaryAsync(h => h.Name);
            var postHashtags = new[]
            {
                new PostHashtag { PostId = alicePost.Id, HashtagId = hashtagDict["photography"].Id },
                new PostHashtag { PostId = alicePost.Id, HashtagId = hashtagDict["goldenhour"].Id },
                new PostHashtag { PostId = alicePost.Id, HashtagId = hashtagDict["travel"].Id },
                new PostHashtag { PostId = carlaPost.Id, HashtagId = hashtagDict["nature"].Id },
                new PostHashtag { PostId = carlaPost.Id, HashtagId = hashtagDict["hiking"].Id },
                new PostHashtag { PostId = carlaPost.Id, HashtagId = hashtagDict["waterfall"].Id },
                new PostHashtag { PostId = bobPost.Id, HashtagId = hashtagDict["dev"].Id },
                new PostHashtag { PostId = bobPost.Id, HashtagId = hashtagDict["coding"].Id },
                new PostHashtag { PostId = bobPost.Id, HashtagId = hashtagDict["startup"].Id },
            };
            _db.PostHashtags.AddRange(postHashtags);

            // Update post counts
            alice.PostCount = 1;
            carla.PostCount = 1;
            bob.PostCount = 1;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seed data created successfully.");
    }
}
