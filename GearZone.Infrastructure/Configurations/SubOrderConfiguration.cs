using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class SubOrderConfiguration : IEntityTypeConfiguration<SubOrder>
    {
        public void Configure(EntityTypeBuilder<SubOrder> builder)
        {
            builder.ToTable("SubOrders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.PayoutStatus)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            builder.Property(x => x.CommissionAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.CommissionRateSnapshot).HasColumnType("decimal(5,2)");
            builder.Property(x => x.DeliveredAt).HasColumnType("datetime2");

            builder.HasIndex(x => new { x.StoreId, x.CreatedAt });
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PayoutStatus);

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.SubOrders)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Store)
                   .WithMany()
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PayoutItem)
                   .WithOne(x => x.SubOrder)
                   .HasForeignKey<PayoutItem>(x => x.SubOrderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
