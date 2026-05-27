using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500);

            builder.HasIndex(x => x.VariantId);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.CreatedByUserId);

            builder.HasOne(x => x.Variant)
                   .WithMany(x => x.InventoryTransactions)
                   .HasForeignKey(x => x.VariantId);

            builder.HasOne(x => x.CreatedByUser)
                   .WithMany()
                   .HasForeignKey(x => x.CreatedByUserId);
        }
    }
}
