using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface ICollectionService
{
    Task<IReadOnlyList<CollectionDto>> GetUserCollectionsAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<PostDto>> GetCollectionPostsAsync(Guid collectionId, Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<CollectionDto> CreateAsync(Guid userId, CreateCollectionRequest request, CancellationToken ct = default);
    Task<CollectionDto> UpdateAsync(Guid collectionId, Guid userId, UpdateCollectionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid collectionId, Guid userId, CancellationToken ct = default);
    Task AddPostAsync(Guid collectionId, Guid postId, Guid userId, CancellationToken ct = default);
    Task RemovePostAsync(Guid collectionId, Guid postId, Guid userId, CancellationToken ct = default);
}
