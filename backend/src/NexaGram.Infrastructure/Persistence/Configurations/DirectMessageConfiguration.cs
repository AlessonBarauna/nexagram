using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaGram.Domain.Entities;

namespace NexaGram.Infrastructure.Persistence.Configurations;

public class DirectMessageConfiguration : IEntityTypeConfiguration<DirectMessage>
{
    public void Configure(EntityTypeBuilder<DirectMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.Content).HasMaxLength(5000);
        builder.Property(m => m.MediaUrl).HasMaxLength(1000);

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.SharedPost)
            .WithMany()
            .HasForeignKey(m => m.SharedPostId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => new { m.SenderId, m.ReceiverId });
        builder.HasIndex(m => m.CreatedAt);
        builder.HasIndex(m => m.ExpiresAt);
    }
}
