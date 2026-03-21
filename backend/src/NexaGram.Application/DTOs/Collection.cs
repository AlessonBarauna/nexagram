namespace NexaGram.Application.DTOs;

public record CollectionDto(
    Guid Id,
    string Name,
    string? CoverUrl,
    int PostCount,
    bool IsPrivate,
    DateTime CreatedAt);

public record CreateCollectionRequest(string Name, bool IsPrivate = false);

public record UpdateCollectionRequest(string? Name, bool? IsPrivate);
