using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Caption).HasMaxLength(2200);
        builder.Property(p => p.Media).HasColumnType("jsonb").IsRequired();
        builder.Property(p => p.Location).HasColumnType("jsonb");
        builder.Property(p => p.AiTags).HasColumnType("jsonb");
        builder.Property(p => p.Embedding).HasColumnType("vector(384)");
        builder.Property(p => p.Visibility).HasConversion<string>();
        builder.Property(p => p.Status).HasConversion<string>();

        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.Status);
    }
}
