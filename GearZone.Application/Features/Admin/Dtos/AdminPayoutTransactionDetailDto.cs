using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutTransactionDetailDto
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        
        // Batch Info
        public Guid PayoutBatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public DateTime BatchCreatedAt { get; set; }

        // Store Info
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;
        public string StorePhone { get; set; } = string.Empty;
        public string StoreOwnerName { get; set; } = string.Empty;
        public decimal StoreCommissionRate { get; set; }

        // Amounts
        public int OrderCount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }

        // Bank Info (Snapshotted)
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankBin { get; set; } = string.Empty;

        public PayoutTransactionStatus Status { get; set; }
        public string? PayOSTransactionId { get; set; }
        public string? FailureReason { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public List<AdminPayoutItemDto> Items { get; set; } = new();
    }
}
