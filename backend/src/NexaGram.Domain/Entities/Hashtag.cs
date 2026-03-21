namespace NexaGram.Domain.Entities;

public class Hashtag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = ""; // unique, lowercase, no #
    public int PostCount { get; set; }

    public ICollection<PostHashtag> PostHashtags { get; set; } = [];
}
