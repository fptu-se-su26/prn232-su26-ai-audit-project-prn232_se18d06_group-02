using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
    {
        public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
        {
            builder.ToTable("VariantAttributeValues");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CategoryAttributeOptionId).IsRequired();

            // Composite unique index to ensure 1 variant has only 1 value per attribute
            builder.HasIndex(x => new { x.VariantId, x.CategoryAttributeId }).IsUnique();
            
            // Index for fast filtering on the Browse page using OptionId
            builder.HasIndex(x => new { x.CategoryAttributeId, x.CategoryAttributeOptionId });

            builder.HasOne(x => x.Variant)
                   .WithMany(x => x.AttributeValues)
                   .HasForeignKey(x => x.VariantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.CategoryAttributeOption)
                   .WithMany(x => x.VariantSelections)
                   .HasForeignKey(x => x.CategoryAttributeOptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CategoryAttribute)
                   .WithMany()
                   .HasForeignKey(x => x.CategoryAttributeId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
