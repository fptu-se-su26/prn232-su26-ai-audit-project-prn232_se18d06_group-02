using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();

            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => new { x.ProductId, x.IsPrimary });

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.Images)
                   .HasForeignKey(x => x.ProductId);
        }
    }
}
