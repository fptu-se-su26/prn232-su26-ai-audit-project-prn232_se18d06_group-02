using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class PromotionReservationConfiguration : IEntityTypeConfiguration<PromotionReservation>
    {
        public void Configure(EntityTypeBuilder<PromotionReservation> builder)
        {
            builder.ToTable("PromotionReservations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            builder.HasIndex(x => x.OrderItemId).IsUnique();
            builder.HasIndex(x => new { x.CampaignId, x.Status });
            builder.HasIndex(x => x.OrderId);

            builder.HasOne(x => x.Campaign)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithMany(x => x.PromotionReservations)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrderItem)
                .WithOne(x => x.PromotionReservation)
                .HasForeignKey<PromotionReservation>(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
