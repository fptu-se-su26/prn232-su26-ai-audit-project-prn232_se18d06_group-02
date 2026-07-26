using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class PromotionCampaignConfiguration : IEntityTypeConfiguration<PromotionCampaign>
    {
        public void Configure(EntityTypeBuilder<PromotionCampaign> builder)
        {
            builder.ToTable("PromotionCampaigns");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.DiscountType).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            builder.Property(x => x.RowVersion).IsRowVersion();

            builder.HasIndex(x => new { x.StoreId, x.StartAt, x.EndAt });
            builder.HasIndex(x => new { x.StoreId, x.IsEnabled });

            builder.HasOne(x => x.Store)
                .WithMany(x => x.PromotionCampaigns)
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
