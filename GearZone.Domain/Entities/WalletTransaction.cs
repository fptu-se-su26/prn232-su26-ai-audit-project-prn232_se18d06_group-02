using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class WalletTransaction : Entity<Guid>
    {
        // Transaction info
        public string TransactionCode { get; set; } = string.Empty;

        public WalletTransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "VND";

        // Balance snapshot
        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        // Direction
        public TransactionDirection Direction { get; set; }

        // Reference
        public string? ReferenceCode { get; set; }
        // Example:
        // BatchCode
        // OrderCode
        // AdminTopupRef

        public Guid? PayoutBatchId { get; set; }

        public Guid? PayoutTransactionId { get; set; }

        public Guid? PaymentId { get; set; }

        // External provider
        public string? Provider { get; set; } // BaoKim

        public string? ProviderTransactionId { get; set; }

        // Status
        public WalletTransactionStatus Status { get; set; }

        // Audit
        public string? CreatedByAdminId { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation
        public PayoutBatch? PayoutBatch { get; set; }

        public PayoutTransaction? PayoutTransaction { get; set; }

        public Payment? Payment { get; set; }
    }
}
