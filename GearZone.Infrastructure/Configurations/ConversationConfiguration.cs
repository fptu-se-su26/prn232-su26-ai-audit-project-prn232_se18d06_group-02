using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            builder.ToTable("Conversations");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.BuyerUserId, x.StoreId }).IsUnique();
            builder.HasIndex(x => x.LastMessageAt);

            builder.HasOne(x => x.BuyerUser)
                   .WithMany(x => x.Conversations)
                   .HasForeignKey(x => x.BuyerUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Store)
                   .WithMany(x => x.Conversations)
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
