using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class CategoryAttributeOptionConfiguration : IEntityTypeConfiguration<CategoryAttributeOption>
    {
        public void Configure(EntityTypeBuilder<CategoryAttributeOption> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Value).IsRequired().HasMaxLength(100);

            builder.HasOne(x => x.CategoryAttribute)
                .WithMany(x => x.Options)
                .HasForeignKey(x => x.CategoryAttributeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
