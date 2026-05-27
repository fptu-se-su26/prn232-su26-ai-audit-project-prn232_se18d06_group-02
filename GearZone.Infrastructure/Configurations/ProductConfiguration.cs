using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
            builder.Property(x => x.Slug).HasMaxLength(300).IsRequired();
            
            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.BasePrice)
                   .HasColumnType("decimal(18,2)");

            builder.HasIndex(x => new { x.StoreId, x.Slug }).IsUnique();
            builder.HasIndex(x => new { x.StoreId, x.Status });
            builder.HasIndex(x => new { x.CategoryId, x.Status });
            builder.HasIndex(x => new { x.BrandId, x.Status });
            builder.HasIndex(x => x.BasePrice);
            builder.HasIndex(x => x.SoldCount);

            builder.HasOne(x => x.Store)
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.StoreId);

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.CategoryId);

            builder.HasOne(x => x.Brand)
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.BrandId);
        }
    }
}
