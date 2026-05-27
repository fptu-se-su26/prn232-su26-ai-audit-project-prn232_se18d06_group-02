using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class PlatformTransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Type and Status labels for display
        public string TypeLabel { get; set; } = string.Empty; // "Payment", "Payout", etc.
        public string StatusLabel { get; set; } = string.Empty; // "Completed", "Pending", etc.
        public string Direction { get; set; } = string.Empty; // "IN", "OUT"
        
        public string? StoreName { get; set; }
        public string? ReferenceCode { get; set; } // OrderCode or BatchCode
        public string? Note { get; set; }
        
        // Original IDs for navigation
        public Guid? PaymentId { get; set; }
        public Guid? WalletTransactionId { get; set; }
        public Guid? PayoutBatchId { get; set; }
    }
}
