using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexaGram.Application.Interfaces;

namespace NexaGram.Infrastructure.Services.Ai;

public class OllamaAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<OllamaAiService> _logger;

    private string VisionModel => _config["Ai:Ollama:VisionModel"] ?? "llava";
    private string TextModel => _config["Ai:Ollama:TextModel"] ?? "llama3.1";
    private string EmbeddingModel => _config["Ai:Ollama:EmbeddingModel"] ?? "nomic-embed-text";

    public OllamaAiService(HttpClient http, IConfiguration config, ILogger<OllamaAiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
        var baseUrl = config["Ai:Ollama:BaseUrl"] ?? "http://localhost:11434";
        _http.BaseAddress = new Uri(baseUrl);
    }

    public async Task<CaptionResult> GenerateCaptionAsync(byte[] imageBytes, string mimeType, string userLanguage = "pt-BR", CancellationToken ct = default)
    {
        var base64 = Convert.ToBase64String(imageBytes);
        var systemPrompt = _config["Ai:Prompts:CaptionSystem"] ?? "Generate a creative caption, hashtags, and alt text.";
        var prompt = $"Generate a creative and engaging social media caption in {userLanguage}, 15 relevant hashtags, and an accessibility alt text for this image. Return valid JSON: {{\"caption\": \"\", \"hashtags\": [], \"altText\": \"\"}}";

        var payload = new
        {
            model = VisionModel,
            prompt,
            images = new[] { base64 },
            system = systemPrompt,
            stream = false,
            format = "json"
        };

        var response = await _http.PostAsJsonAsync("/api/generate", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
        var json = JsonSerializer.Deserialize<JsonElement>(result?.Response ?? "{}");

        return new CaptionResult(
            json.TryGetProperty("caption", out var cap) ? cap.GetString() ?? "" : "",
            json.TryGetProperty("hashtags", out var tags)
                ? tags.EnumerateArray().Select(t => t.GetString() ?? "").ToArray()
                : [],
            json.TryGetProperty("altText", out var alt) ? alt.GetString() ?? "" : ""
        );
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var payload = new { model = EmbeddingModel, prompt = text };
        var response = await _http.PostAsJsonAsync("/api/embeddings", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: ct);
        return result?.Embedding ?? [];
    }

    public async Task<ModerationResult> ModerateAsync(byte[]? imageBytes, string? mimeType, string? text, CancellationToken ct = default)
    {
        var systemPrompt = _config["Ai:Prompts:ModerationSystem"] ?? "Moderate content.";
        var prompt = $"Analyze this content for policy violations. Text: \"{text ?? "none"}\". Return JSON: {{\"safe\": true, \"categories\": [], \"confidence\": 0.99}}";

        object payload;
        if (imageBytes != null)
        {
            var base64 = Convert.ToBase64String(imageBytes);
            payload = new { model = VisionModel, prompt, images = new[] { base64 }, system = systemPrompt, stream = false, format = "json" };
        }
        else
        {
            payload = new { model = TextModel, prompt, system = systemPrompt, stream = false, format = "json" };
        }

        var response = await _http.PostAsJsonAsync("/api/generate", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
        var json = JsonSerializer.Deserialize<JsonElement>(result?.Response ?? "{\"safe\":true,\"categories\":[],\"confidence\":1.0}");

        return new ModerationResult(
            json.TryGetProperty("safe", out var safe) && safe.GetBoolean(),
            json.TryGetProperty("categories", out var cats)
                ? cats.EnumerateArray().Select(c => c.GetString() ?? "").ToArray()
                : [],
            json.TryGetProperty("confidence", out var conf) ? conf.GetDouble() : 1.0
        );
    }

    public async Task<HashtagResult[]> GenerateHashtagsAsync(string caption, CancellationToken ct = default)
    {
        var systemPrompt = _config["Ai:Prompts:HashtagSystem"] ?? "Generate hashtags.";
        var prompt = $"Generate relevant hashtags for: \"{caption}\". Return JSON array: [{{\"tag\": \"#example\", \"relevance\": 0.9}}]";

        var payload = new { model = TextModel, prompt, system = systemPrompt, stream = false, format = "json" };
        var response = await _http.PostAsJsonAsync("/api/generate", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
        try
        {
            var tags = JsonSerializer.Deserialize<HashtagResult[]>(result?.Response ?? "[]");
            return tags ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<string> CompanionChatAsync(string userMessage, string systemContext, CancellationToken ct = default)
    {
        var systemPrompt = _config["Ai:Prompts:CompanionSystem"] ?? "You are Nexia, a helpful AI companion.";
        var payload = new
        {
            model = TextModel,
            prompt = userMessage,
            system = $"{systemPrompt}\n\nUser context: {systemContext}",
            stream = false
        };

        var response = await _http.PostAsJsonAsync("/api/generate", payload, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(cancellationToken: ct);
        return result?.Response ?? "I'm sorry, I couldn't process your request.";
    }

    private record OllamaResponse(string Response);
    private record OllamaEmbeddingResponse(float[] Embedding);
}
