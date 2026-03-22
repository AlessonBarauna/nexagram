using NexaGram.Infrastructure.Services.Storage;

namespace NexaGram.API.Extensions;

public static class StorageExtensions
{
    public static async Task EnsureStorageReadyAsync(this WebApplication app)
    {
        var provider = app.Configuration["Storage:Provider"] ?? "minio";
        if (provider.Equals("local", StringComparison.OrdinalIgnoreCase))
        {
            app.Logger.LogInformation("Storage: using local file system.");
            return;
        }

        using var scope = app.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<MinioStorageService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MinioStorageService>>();

        try
        {
            await storage.EnsureBucketExistsAsync();
            logger.LogInformation("MinIO bucket ready.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MinIO not available — storage features will be unavailable.");
        }
    }
}
