namespace NexaGram.Application.DTOs;

public record DirectMessageDto(
    Guid Id,
    UserSummaryDto Sender,
    UserSummaryDto Receiver,
    string? Content,
    string? MediaUrl,
    PostDto? SharedPost,
    bool IsRead,
    bool IsEphemeral,
    DateTime? ExpiresAt,
    DateTime CreatedAt);

public record SendMessageRequest(
    string RecipientUsername,
    string? Content,
    string? MediaKey,
    Guid? SharedPostId,
    bool IsEphemeral = false,
    int? EphemeralSeconds = null);

public record ConversationDto(
    UserSummaryDto Participant,
    DirectMessageDto LastMessage,
    int UnreadCount);
