namespace NexaGram.Application.DTOs;

public record AiCaptionRequest(string MediaKey, string Language = "pt-BR");

public record AiCaptionResponse(string Caption, string[] Hashtags, string AltText);

public record AiModerationRequest(string? MediaKey, string? Text);

public record AiModerationResponse(bool Safe, string[] Categories, double Confidence);

public record AiHashtagRequest(string Caption);

public record AiCompanionRequest(string Message, string? Context = null);
