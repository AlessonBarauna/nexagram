using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace NexaGram.Infrastructure.Persistence;

/// <summary>
/// Provides a design-time factory for AppDbContext so that dotnet-ef migrations
/// can be run without starting the full application host (which requires Redis, etc.).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=nexagram_dev;Username=postgres;Password=postgres",
            o => o.UseVector());

        return new AppDbContext(optionsBuilder.Options);
    }
}
