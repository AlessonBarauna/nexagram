using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence.Configurations;

public class SaveConfiguration : IEntityTypeConfiguration<Save>
{
    public void Configure(EntityTypeBuilder<Save> builder)
    {
        builder.HasKey(s => new { s.UserId, s.PostId });

        builder.HasOne(s => s.User)
            .WithMany(u => u.Saves)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Post)
            .WithMany(p => p.Saves)
            .HasForeignKey(s => s.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Collection)
            .WithMany(c => c.Saves)
            .HasForeignKey(s => s.CollectionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
