using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderCode).HasMaxLength(50).IsRequired();

            builder.HasIndex(x => x.OrderCode).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.CreatedAt });
            builder.HasIndex(x => new { x.UserId, x.CheckoutRequestId })
                   .IsUnique()
                   .HasFilter("[CheckoutRequestId] IS NOT NULL");

            builder.HasOne(x => x.User)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.OrderDiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.ShippingDiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.OrderVoucherCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.ShippingVoucherCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.OrderVoucherScopeSnapshot).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.ShippingVoucherScopeSnapshot).HasConversion<string>().HasMaxLength(30);

            builder.HasOne(x => x.OrderVoucher)
                   .WithMany()
                   .HasForeignKey(x => x.OrderVoucherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ShippingVoucher)
                   .WithMany()
                   .HasForeignKey(x => x.ShippingVoucherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Shipments)
                   .WithOne(s => s.Order)
                   .HasForeignKey(s => s.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
