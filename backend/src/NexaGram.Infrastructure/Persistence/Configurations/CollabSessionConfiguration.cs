using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence.Configurations;

public class CollabSessionConfiguration : IEntityTypeConfiguration<CollabSession>
{
    public void Configure(EntityTypeBuilder<CollabSession> builder)
    {
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(cs => cs.Title).IsRequired().HasMaxLength(200);
        builder.Property(cs => cs.CanvasState).HasColumnType("jsonb");
        builder.Property(cs => cs.ParticipantIds).HasColumnType("jsonb").IsRequired();
        builder.Property(cs => cs.Status).HasConversion<string>();

        builder.HasOne(cs => cs.Creator)
            .WithMany()
            .HasForeignKey(cs => cs.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.ResultPost)
            .WithMany()
            .HasForeignKey(cs => cs.ResultPostId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
