using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class PayoutItemConfiguration : IEntityTypeConfiguration<PayoutItem>
    {
        public void Configure(EntityTypeBuilder<PayoutItem> builder)
        {
            builder.ToTable("PayoutItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GrandTotal)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.CommissionAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.NetAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.ExcludeReason)
                .HasMaxLength(500);

            builder.HasOne(x => x.Transaction)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PayoutTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SubOrder)
                .WithOne(x => x.PayoutItem)
                .HasForeignKey<PayoutItem>(x => x.SubOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
