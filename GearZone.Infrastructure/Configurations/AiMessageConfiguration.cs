using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations;

public sealed class AiMessageConfiguration : IEntityTypeConfiguration<AiMessage>
{
    public void Configure(EntityTypeBuilder<AiMessage> builder)
    {
        builder.ToTable("AiMessages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        builder.Property(x => x.Content).HasMaxLength(12000).IsRequired();
        builder.Property(x => x.MetadataJson).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(120);

        builder.HasIndex(x => new { x.ConversationId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.ConversationId, x.ClientMessageId, x.Role }).IsUnique();
    }
}
