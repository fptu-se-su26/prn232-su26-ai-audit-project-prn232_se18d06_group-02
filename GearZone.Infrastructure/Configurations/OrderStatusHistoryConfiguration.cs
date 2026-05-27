using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("OrderStatusHistories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OldStatus)
                   .HasConversion<string>()
                   .HasMaxLength(30);

            builder.Property(x => x.NewStatus)
                   .HasConversion<string>()
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => new { x.OrderId, x.ChangedAt });
            builder.HasIndex(x => x.ChangedByUserId);
            builder.Property(x => x.ChangedByUserId).IsRequired(false);

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.StatusHistories)
                   .HasForeignKey(x => x.OrderId);

            builder.HasOne(x => x.ChangedByUser)
                   .WithMany()
                   .HasForeignKey(x => x.ChangedByUserId)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
