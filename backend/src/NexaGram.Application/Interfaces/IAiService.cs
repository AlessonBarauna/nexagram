namespace NexaGram.Application.Interfaces;

public interface IAiService
{
    /// <summary>Generates caption, hashtags, and alt text for an image.</summary>
    Task<CaptionResult> GenerateCaptionAsync(byte[] imageBytes, string mimeType, string userLanguage = "pt-BR", CancellationToken ct = default);

    /// <summary>Generates embeddings for semantic search.</summary>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);

    /// <summary>Moderates image and/or text content.</summary>
    Task<ModerationResult> ModerateAsync(byte[]? imageBytes, string? mimeType, string? text, CancellationToken ct = default);

    /// <summary>Generates relevant hashtags for a given caption.</summary>
    Task<HashtagResult[]> GenerateHashtagsAsync(string caption, CancellationToken ct = default);

    /// <summary>Sends a message to the Nexia AI companion.</summary>
    Task<string> CompanionChatAsync(string userMessage, string systemContext, CancellationToken ct = default);
}

public record CaptionResult(string Caption, string[] Hashtags, string AltText);
public record ModerationResult(bool Safe, string[] Categories, double Confidence);
public record HashtagResult(string Tag, double Relevance);
