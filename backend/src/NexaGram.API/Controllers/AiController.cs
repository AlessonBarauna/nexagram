using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.DTOs;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _ai;
    private readonly IStorageService _storage;

    public AiController(IAiService ai, IStorageService storage)
    {
        _ai = ai;
        _storage = storage;
    }

    /// <summary>Generate caption, hashtags and alt text for an uploaded image.</summary>
    [HttpPost("caption")]
    public async Task<ActionResult<AiCaptionResponse>> GenerateCaption(
        AiCaptionRequest request, CancellationToken ct)
    {
        var imageBytes = await _storage.DownloadAsync(request.MediaKey, ct);
        var mimeType = InferMimeType(request.MediaKey);
        var result = await _ai.GenerateCaptionAsync(imageBytes, mimeType, request.Language, ct);
        return Ok(new AiCaptionResponse(result.Caption, result.Hashtags, result.AltText));
    }

    /// <summary>Moderate image and/or text for unsafe content.</summary>
    [HttpPost("moderate")]
    public async Task<ActionResult<AiModerationResponse>> Moderate(
        AiModerationRequest request, CancellationToken ct)
    {
        byte[]? imageBytes = null;
        string? mimeType = null;

        if (request.MediaKey is not null)
        {
            imageBytes = await _storage.DownloadAsync(request.MediaKey, ct);
            mimeType = InferMimeType(request.MediaKey);
        }

        var result = await _ai.ModerateAsync(imageBytes, mimeType, request.Text, ct);
        return Ok(new AiModerationResponse(result.Safe, result.Categories, result.Confidence));
    }

    /// <summary>Generate relevant hashtags for a caption.</summary>
    [HttpPost("hashtags")]
    public async Task<ActionResult<HashtagResult[]>> GenerateHashtags(
        AiHashtagRequest request, CancellationToken ct)
    {
        var result = await _ai.GenerateHashtagsAsync(request.Caption, ct);
        return Ok(result);
    }

    /// <summary>Chat with Nexia, the AI companion.</summary>
    [HttpPost("companion")]
    public async Task<ActionResult<string>> Companion(
        AiCompanionRequest request, CancellationToken ct)
    {
        var systemContext = request.Context ?? "You are Nexia, a helpful AI companion for a social media creator.";
        var reply = await _ai.CompanionChatAsync(request.Message, systemContext, ct);
        return Ok(reply);
    }

    private static string InferMimeType(string key)
    {
        var ext = Path.GetExtension(key).ToLower();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };
    }
}
