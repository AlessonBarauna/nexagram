using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NexaGram.Application.Interfaces;

namespace NexaGram.Infrastructure.Services.Storage;

/// <summary>
/// Development-only storage: saves files to wwwroot/uploads and serves them as static files.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration config, IWebHostEnvironment env)
    {
        _uploadsPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads");
        Directory.CreateDirectory(_uploadsPath);
        _baseUrl = config["Storage:LocalBaseUrl"] ?? "http://localhost:5245/uploads";
    }

    public Task<PresignedUploadResult> GeneratePresignedUploadUrlAsync(string key, string mimeType, CancellationToken ct = default)
    {
        // Not used in local mode — direct upload endpoint is used instead
        var result = new PresignedUploadResult("", key, DateTimeOffset.UtcNow.AddHours(1));
        return Task.FromResult(result);
    }

    public string GetPublicUrl(string key)
    {
        var fileName = Path.GetFileName(key);
        return $"{_baseUrl}/{fileName}";
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var path = Path.Combine(_uploadsPath, Path.GetFileName(key));
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public async Task<byte[]> DownloadAsync(string key, CancellationToken ct = default)
    {
        var path = Path.Combine(_uploadsPath, Path.GetFileName(key));
        return await File.ReadAllBytesAsync(path, ct);
    }

    public async Task<string> UploadAsync(string key, byte[] data, string mimeType, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(key);
        var path = Path.Combine(_uploadsPath, fileName);
        await File.WriteAllBytesAsync(path, data, ct);
        return GetPublicUrl(key);
    }
}
