using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PaymentCode)
                   .HasMaxLength(50);

            builder.Property(x => x.Method)
                   .HasMaxLength(20)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(x => x.Status)
                   .HasMaxLength(20)
                   .IsRequired()
                   .HasConversion<string>();
            builder.Property(x => x.Provider).HasMaxLength(50);
            builder.Property(x => x.TransactionRef).HasMaxLength(200);

            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.TransactionRef);

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.Payments)
                   .HasForeignKey(x => x.OrderId);
        }
    }
}
