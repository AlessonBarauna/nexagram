using NexaGram.Application.Interfaces;

namespace NexaGram.Infrastructure.Services.Ai;

/// <summary>
/// Claude AI service - activate by setting Ai:Provider = "claude" and providing ANTHROPIC_API_KEY.
/// Follows the same IAiService interface so no business logic changes are needed.
/// </summary>
public class ClaudeAiService : IAiService
{
    public Task<CaptionResult> GenerateCaptionAsync(byte[] imageBytes, string mimeType, string userLanguage = "pt-BR", CancellationToken ct = default)
        => throw new NotImplementedException("ClaudeAiService not yet implemented. Set Ai:Provider = 'ollama' for free usage.");

    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
        => throw new NotImplementedException("ClaudeAiService not yet implemented.");

    public Task<ModerationResult> ModerateAsync(byte[]? imageBytes, string? mimeType, string? text, CancellationToken ct = default)
        => throw new NotImplementedException("ClaudeAiService not yet implemented.");

    public Task<HashtagResult[]> GenerateHashtagsAsync(string caption, CancellationToken ct = default)
        => throw new NotImplementedException("ClaudeAiService not yet implemented.");

    public Task<string> CompanionChatAsync(string userMessage, string systemContext, CancellationToken ct = default)
        => throw new NotImplementedException("ClaudeAiService not yet implemented.");
}
