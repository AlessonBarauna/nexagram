using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexaGram.Application.Interfaces;

namespace NexaGram.API.Controllers;

[ApiController]
[Route("api/storage")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

    /// <summary>
    /// Direct file upload (alternative to presigned URL, for environments without MinIO CORS).
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var allowed = _config.GetSection("Storage:AllowedMimeTypes").Get<string[]>()
            ?? ["image/jpeg", "image/png", "image/webp", "image/gif", "video/mp4"];

        if (!allowed.Contains(file.ContentType))
            return BadRequest($"File type '{file.ContentType}' is not allowed.");

        var maxSize = _config.GetValue<long>("Storage:MaxFileSizeBytes", 52_428_800);
        if (file.Length > maxSize)
            return BadRequest($"File exceeds maximum size of {maxSize / 1_048_576}MB.");

        var ext = file.ContentType.Split('/').Last();
        var key = $"media/{Guid.NewGuid()}.{ext}";

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var url = await _storage.UploadAsync(key, ms.ToArray(), file.ContentType, ct);

        return Ok(new { key, url, mimeType = file.ContentType });
    }
}

public record PresignRequest(string MimeType);
