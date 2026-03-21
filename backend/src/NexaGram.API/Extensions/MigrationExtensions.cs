using Microsoft.EntityFrameworkCore;
using NexaGram.Infrastructure.Persistence;

namespace NexaGram.API.Extensions;

public static class MigrationExtensions
{
    public static async Task RunMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        app.Logger.LogInformation("Running database migrations...");
        await db.Database.MigrateAsync();

        if (app.Environment.IsDevelopment())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync();
        }
    }
}
