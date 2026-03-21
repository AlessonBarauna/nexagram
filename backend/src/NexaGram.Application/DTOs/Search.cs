namespace NexaGram.Application.DTOs;

public record HashtagDto(Guid Id, string Name, int PostCount);

public record SearchResultDto(
    IReadOnlyList<UserSummaryDto> Users,
    IReadOnlyList<HashtagDto> Hashtags,
    IReadOnlyList<PostDto> Posts);
