using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence.Configurations;

public class HashtagConfiguration : IEntityTypeConfiguration<Hashtag>
{
    public void Configure(EntityTypeBuilder<Hashtag> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(h => h.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(h => h.Name).IsUnique();
    }
}
