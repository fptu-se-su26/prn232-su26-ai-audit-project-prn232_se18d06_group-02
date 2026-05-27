using GearZone.Domain.Enums;
using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutBatchDto
    {
        public Guid Id { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
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
        public PayoutBatchStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Derived
        public List<AdminPayoutTransactionDto> Transactions { get; set; } = new();
    }
}
