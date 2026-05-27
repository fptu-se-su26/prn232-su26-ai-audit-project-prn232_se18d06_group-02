using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class PayoutTransactionConfiguration : IEntityTypeConfiguration<PayoutTransaction>
    {
        public void Configure(EntityTypeBuilder<PayoutTransaction> builder)
        {
            builder.ToTable("PayoutTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionCode)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.BankName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.BankAccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.BankAccountName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.BankBin)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.GrossAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.CommissionAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.NetAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Status)
                   .HasMaxLength(20)
                   .IsRequired()
                   .HasConversion<string>();

            builder.Property(x => x.PayOSTransactionId)
                .HasMaxLength(100);

            builder.Property(x => x.FailureReason)
                .HasMaxLength(500);

            builder.Property(x => x.ExcludeReason)
                .HasMaxLength(500);

            builder.HasOne(x => x.Batch)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.PayoutBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Transaction)
                .HasForeignKey(x => x.PayoutTransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
