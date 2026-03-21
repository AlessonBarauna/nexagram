using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/storage")]
[Authorize]
public class StorageController : ControllerBase
{
    private readonly IStorageService _storage;
    private readonly IConfiguration _config;

    public StorageController(IStorageService storage, IConfiguration config)
    {
        _storage = storage;
        _config = config;
    }

    /// <summary>
    /// Generates a presigned URL for direct client upload to MinIO.
    /// The client uploads the file directly, then uses the returned key when creating a post.
    /// </summary>
    [HttpPost("presign")]
    public async Task<IActionResult> Presign([FromBody] PresignRequest request, CancellationToken ct)
    {
        var allowed = _config.GetSection("Storage:AllowedMimeTypes").Get<string[]>()
            ?? ["image/jpeg", "image/png", "image/webp", "image/gif", "video/mp4"];

        if (!allowed.Contains(request.MimeType))
            return BadRequest($"MimeType '{request.MimeType}' is not allowed.");

        var ext = request.MimeType.Split('/').Last();
        var key = $"media/{Guid.NewGuid()}.{ext}";

        var result = await _storage.GeneratePresignedUploadUrlAsync(key, request.MimeType, ct);
        return Ok(result);
    }
}

public record PresignRequest(string MimeType);
