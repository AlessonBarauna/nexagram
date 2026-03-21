using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using NexaGram.Application.Interfaces;

namespace NexaGram.Infrastructure.Services.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _publicEndpoint;

    public MinioStorageService(IConfiguration config)
    {
        var section = config.GetSection("Storage");
        _bucket = section["BucketName"] ?? "nexagram-media";
        _publicEndpoint = section["Endpoint"] ?? "http://localhost:9000";

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _publicEndpoint,
            ForcePathStyle = section.GetValue<bool>("UsePathStyle", true),
        };

        _s3 = new AmazonS3Client(
            new BasicAWSCredentials(
                section["AccessKey"] ?? "nexagram",
                section["SecretKey"] ?? "nexagram_dev"),
            s3Config);
    }

    public async Task EnsureBucketExistsAsync()
    {
        try
        {
            await _s3.EnsureBucketExistsAsync(_bucket);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not ensure MinIO bucket '{_bucket}' exists: {ex.Message}", ex);
        }
    }

    public async Task<PresignedUploadResult> GeneratePresignedUploadUrlAsync(string key, string mimeType, CancellationToken ct = default)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = mimeType,
            Expires = expiresAt.UtcDateTime,
        };

        var url = await _s3.GetPreSignedURLAsync(request);
        return new PresignedUploadResult(url, key, expiresAt);
    }

    public string GetPublicUrl(string key) => $"{_publicEndpoint}/{_bucket}/{key}";

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_bucket, key, ct);
    }

    public async Task<byte[]> DownloadAsync(string key, CancellationToken ct = default)
    {
        var response = await _s3.GetObjectAsync(_bucket, key, ct);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    public async Task<string> UploadAsync(string key, byte[] data, string mimeType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream(data);
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = ms,
            ContentType = mimeType,
            DisablePayloadSigning = true,
        }, ct);

        return GetPublicUrl(key);
    }
}
