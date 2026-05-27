using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable("Vouchers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
            
            builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.DiscountType).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Scope).HasConversion<string>().HasMaxLength(30);
            builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            builder.Property(x => x.DiscountValue).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MaxDiscount).HasColumnType("decimal(18,2)");
            builder.Property(x => x.MinOrderAmount).HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.Code).IsUnique();

            builder.HasOne(x => x.Store)
                   .WithMany()
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Category)
                   .WithMany()
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
