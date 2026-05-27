using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class PayoutBatchConfiguration : IEntityTypeConfiguration<PayoutBatch>
    {
        public void Configure(EntityTypeBuilder<PayoutBatch> builder)
        {
            builder.ToTable("PayoutBatches");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BatchCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.TotalGrossAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.TotalCommissionAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.TotalNetAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Status)
                   .HasMaxLength(20)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(x => x.HoldReason)
                   .HasMaxLength(500);

            builder.Property(x => x.ApprovedByAdminId)
                .HasMaxLength(450); // Typical Identity string length

            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.Batch)
                .HasForeignKey(x => x.PayoutBatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
