using System;
using System.Collections.Generic;
using System.Text;
using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class PayoutTransaction : Entity<Guid>
    {
        public Guid PayoutBatchId { get; set; }
        public Guid StoreId { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        // Bank snapshot
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountName { get; set; }
        public string BankBin { get; set; }
        public int OrderCount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public PayoutTransactionStatus Status { get; set; }
        public string? PayOSTransactionId { get; set; }
        public string? FailureReason { get; set; }
        public string? ExcludeReason { get; set; }
        public int RetryCount { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public PayoutBatch Batch { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public ICollection<PayoutItem> Items { get; set; }
    }
}
