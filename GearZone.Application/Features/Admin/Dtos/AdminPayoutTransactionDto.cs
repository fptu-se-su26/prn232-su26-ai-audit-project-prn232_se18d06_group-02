using GearZone.Domain.Enums;
using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutTransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        
        // Store Info
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreEmail { get; set; } = string.Empty;

        // Amounts
        public int OrderCount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }

        // Bank Info
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;

        public PayoutTransactionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
