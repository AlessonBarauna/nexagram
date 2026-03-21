using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexaGram.Application.Interfaces;

namespace NexaGram.Infrastructure.Services.Ai;

/// <summary>
/// Claude AI service — activate by setting Ai:Provider = "claude" and providing Ai:Claude:ApiKey.
/// </summary>
public class ClaudeAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<ClaudeAiService> _logger;

    private string Model => _config["Ai:Claude:Model"] ?? "claude-sonnet-4-6";
    private string ApiKey => _config["Ai:Claude:ApiKey"] ?? "";

    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ClaudeAiService(HttpClient http, IConfiguration config, ILogger<ClaudeAiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
        _http.BaseAddress = new Uri("https://api.anthropic.com");
        _http.DefaultRequestHeaders.Add("x-api-key", ApiKey);
        _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<CaptionResult> GenerateCaptionAsync(byte[] imageBytes, string mimeType, string userLanguage = "pt-BR", CancellationToken ct = default)
    {
        var base64 = Convert.ToBase64String(imageBytes);
        var systemPrompt = _config["Ai:Prompts:CaptionSystem"] ?? "Generate a creative caption.";
        var userPrompt = $"Generate a creative social media caption in {userLanguage}, 15 relevant hashtags, and an alt text for this image. Return JSON: {{\"caption\":\"\",\"hashtags\":[],\"altText\":\"\"}}";

        var response = await SendMessageAsync(systemPrompt, userPrompt, base64, mimeType, ct);
        var json = JsonSerializer.Deserialize<JsonElement>(response);

        return new CaptionResult(
            json.GetProperty("caption").GetString() ?? "",
            json.GetProperty("hashtags").EnumerateArray().Select(h => h.GetString()!).ToArray(),
            json.GetProperty("altText").GetString() ?? "");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        // Claude doesn't have an embeddings API — fallback to returning empty array
        // For embeddings, use Ollama (nomic-embed-text) which is free and local
        _logger.LogWarning("ClaudeAiService does not support embeddings. Use Ollama for embeddings.");
        return [];
    }

    public async Task<ModerationResult> ModerateAsync(byte[]? imageBytes, string? mimeType, string? text, CancellationToken ct = default)
    {
        var systemPrompt = _config["Ai:Prompts:ModerationSystem"] ?? "You are a content moderation AI.";
        var userPrompt = $"Analyze this content for safety. Content: {text ?? "(image only)"}. Return JSON: {{\"safe\":true,\"categories\":[],\"confidence\":0.99}}";

        string? base64 = imageBytes is not null ? Convert.ToBase64String(imageBytes) : null;
        var response = await SendMessageAsync(systemPrompt, userPrompt, base64, mimeType, ct);
        var json = JsonSerializer.Deserialize<JsonElement>(response);

        return new ModerationResult(
            json.GetProperty("safe").GetBoolean(),
            json.GetProperty("categories").EnumerateArray().Select(c => c.GetString()!).ToArray(),
            json.GetProperty("confidence").GetDouble());
    }

    public async Task<HashtagResult[]> GenerateHashtagsAsync(string caption, CancellationToken ct = default)
    {
        var systemPrompt = _config["Ai:Prompts:HashtagSystem"] ?? "Generate relevant hashtags.";
        var userPrompt = $"Generate 20 relevant hashtags for: \"{caption}\". Return JSON array: [{{\"tag\":\"example\",\"relevance\":0.9}}]";

        var response = await SendMessageAsync(systemPrompt, userPrompt, null, null, ct);
        var items = JsonSerializer.Deserialize<JsonElement[]>(response) ?? [];

        return items.Select(i => new HashtagResult(
            i.GetProperty("tag").GetString() ?? "",
            i.GetProperty("relevance").GetDouble())).ToArray();
    }

    public async Task<string> CompanionChatAsync(string userMessage, string systemContext, CancellationToken ct = default)
    {
        return await SendMessageAsync(systemContext, userMessage, null, null, ct);
    }

    // ── helpers ────────────────────────────────────────────────────────────

    private async Task<string> SendMessageAsync(
        string system, string userText, string? base64Image, string? mimeType, CancellationToken ct)
    {
        var content = new List<object>();

        if (base64Image is not null)
            content.Add(new { type = "image", source = new { type = "base64", media_type = mimeType, data = base64Image } });

        content.Add(new { type = "text", text = userText });

        var payload = new
        {
            model = Model,
            max_tokens = 1024,
            system,
            messages = new[] { new { role = "user", content } }
        };

        var response = await _http.PostAsJsonAsync("/v1/messages", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return result.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
    }
}
