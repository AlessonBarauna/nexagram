using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence.Configurations;

public class StoryViewConfiguration : IEntityTypeConfiguration<StoryView>
{
    public void Configure(EntityTypeBuilder<StoryView> builder)
    {
        builder.HasKey(sv => sv.Id);
        builder.Property(sv => sv.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.HasOne(sv => sv.Story)
            .WithMany(s => s.Views)
            .HasForeignKey(sv => sv.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sv => sv.User)
            .WithMany()
            .HasForeignKey(sv => sv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(sv => new { sv.StoryId, sv.UserId }).IsUnique();
    }
}
