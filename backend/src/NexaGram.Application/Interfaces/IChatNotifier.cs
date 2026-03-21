using NexaGram.Application.DTOs;

namespace NexaGram.Application.Interfaces;

public interface IChatNotifier
{
    Task SendMessageAsync(Guid recipientId, DirectMessageDto message, CancellationToken ct = default);
}
