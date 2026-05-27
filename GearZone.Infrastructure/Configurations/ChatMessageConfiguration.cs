using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            builder.HasIndex(x => x.ConversationId);
            builder.HasIndex(x => x.SentAt);
            builder.HasIndex(x => new { x.ConversationId, x.SentAt });
            builder.HasIndex(x => new { x.ConversationId, x.IsRead, x.SentAt });

            builder.HasOne(x => x.Conversation)
                   .WithMany(x => x.Messages)
                   .HasForeignKey(x => x.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.SenderUser)
                   .WithMany()
                   .HasForeignKey(x => x.SenderUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
