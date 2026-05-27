using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.ToTable("ProductAttributeValues");

            builder.HasKey(x => x.Id);

            // Composite index for fast lookups by product and attribute
            builder.HasIndex(x => new { x.ProductId, x.CategoryAttributeId }).IsUnique();

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.AttributeValues)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CategoryAttribute)
                   .WithMany()
                   .HasForeignKey(x => x.CategoryAttributeId)
                   .OnDelete(DeleteBehavior.NoAction); // Prevent multiple cascade paths

            builder.HasOne(x => x.CategoryAttributeOption)
                   .WithMany()
                   .HasForeignKey(x => x.CategoryAttributeOptionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
