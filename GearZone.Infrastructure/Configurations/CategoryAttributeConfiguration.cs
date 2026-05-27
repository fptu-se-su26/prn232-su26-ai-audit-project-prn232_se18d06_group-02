using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
    {
        public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
        {
            builder.ToTable("CategoryAttributes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.FilterType).HasMaxLength(50).IsRequired();
            
            builder.HasIndex(x => new { x.CategoryId, x.IsFilterable });

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.Attributes)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Options)
                   .WithOne(x => x.CategoryAttribute)
                   .HasForeignKey(x => x.CategoryAttributeId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
