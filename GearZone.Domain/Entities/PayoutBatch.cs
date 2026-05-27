using System;
using System.Collections.Generic;
using System.Text;
using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class PayoutBatch : Entity<Guid>
    {
        public string BatchCode { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public PayoutBatchStatus Status { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public int TotalStores { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public string? HoldReason { get; set; }
        public string? ApprovedByAdminId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<PayoutTransaction> Transactions { get; set; }
    }
}
