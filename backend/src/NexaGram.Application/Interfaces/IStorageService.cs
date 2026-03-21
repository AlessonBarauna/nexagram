namespace NexaGram.Application.Interfaces;

public interface IStorageService
{
    /// <summary>Generates a presigned URL for direct client upload.</summary>
    Task<PresignedUploadResult> GeneratePresignedUploadUrlAsync(string key, string mimeType, CancellationToken ct = default);

    /// <summary>Gets the public URL for an object.</summary>
    string GetPublicUrl(string key);

    /// <summary>Deletes an object from storage.</summary>
    Task DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>Downloads an object as bytes (for processing).</summary>
    Task<byte[]> DownloadAsync(string key, CancellationToken ct = default);

    /// <summary>Uploads bytes directly (used by background jobs).</summary>
    Task<string> UploadAsync(string key, byte[] data, string mimeType, CancellationToken ct = default);
}

public record PresignedUploadResult(string UploadUrl, string Key, DateTimeOffset ExpiresAt);
