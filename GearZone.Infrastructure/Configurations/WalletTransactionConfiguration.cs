using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GearZone.Infrastructure.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TransactionCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BalanceBefore)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BalanceAfter)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("VND");

            builder.Property(x => x.ReferenceCode)
                .HasMaxLength(100);

            builder.Property(x => x.Provider)
                .HasMaxLength(50);

            builder.Property(x => x.ProviderTransactionId)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            builder.Property(x => x.Note)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedByAdminId)
                .HasMaxLength(450);

            // Relationships
            builder.HasOne(x => x.PayoutBatch)
                .WithMany()
                .HasForeignKey(x => x.PayoutBatchId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.PayoutTransaction)
                .WithMany()
                .HasForeignKey(x => x.PayoutTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Payment)
                .WithMany()
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
