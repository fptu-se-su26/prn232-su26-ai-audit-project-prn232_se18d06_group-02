using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.OriginalUnitPriceSnapshot).HasColumnType("decimal(18,2)");
            builder.Property(x => x.UnitPriceSnapshot).HasColumnType("decimal(18,2)");
            builder.Property(x => x.PromotionDiscountPerUnit).HasColumnType("decimal(18,2)");
            builder.Property(x => x.PromotionDiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
            builder.Property(x => x.PromotionNameSnapshot).HasMaxLength(150);

            builder.HasOne(x => x.SubOrder)
                   .WithMany(x => x.Items)
                   .HasForeignKey(x => x.SubOrderId);

            builder.HasOne(x => x.Variant)
                   .WithMany()
                   .HasForeignKey(x => x.VariantId);

            builder.HasOne(x => x.PromotionCampaign)
                   .WithMany()
                   .HasForeignKey(x => x.PromotionCampaignId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
