using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IDirectMessageService
{
    Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<PagedResult<DirectMessageDto>> GetMessagesAsync(Guid userId, Guid participantId, int page, int pageSize, CancellationToken ct = default);
    Task<DirectMessageDto> SendAsync(Guid senderId, SendMessageRequest request, CancellationToken ct = default);
    Task MarkReadAsync(Guid messageId, Guid userId, CancellationToken ct = default);
    Task DeleteAsync(Guid messageId, Guid userId, CancellationToken ct = default);
}
