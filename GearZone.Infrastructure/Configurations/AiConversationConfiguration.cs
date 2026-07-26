using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations;

public sealed class AiConversationConfiguration : IEntityTypeConfiguration<AiConversation>
{
    public void Configure(EntityTypeBuilder<AiConversation> builder)
    {
        builder.ToTable("AiConversations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerUserId).HasMaxLength(450);
        builder.Property(x => x.GuestTokenHash).HasMaxLength(64);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.CustomerUserId, x.LastActivityAtUtc });
        builder.HasIndex(x => x.GuestTokenHash);
        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.HasOne(x => x.CustomerUser)
            .WithMany()
            .HasForeignKey(x => x.CustomerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
