using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LogoUrl).HasMaxLength(500);

            builder.HasIndex(x => x.Slug).IsUnique();
            builder.HasIndex(x => x.IsApproved);
        }
    }
}
