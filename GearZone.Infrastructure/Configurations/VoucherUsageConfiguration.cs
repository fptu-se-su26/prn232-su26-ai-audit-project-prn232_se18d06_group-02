using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class VoucherUsageConfiguration : IEntityTypeConfiguration<VoucherUsage>
    {
        public void Configure(EntityTypeBuilder<VoucherUsage> builder)
        {
            builder.ToTable("VoucherUsages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Voucher)
                   .WithMany(x => x.Usages)
                   .HasForeignKey(x => x.VoucherId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                   .WithMany()
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
