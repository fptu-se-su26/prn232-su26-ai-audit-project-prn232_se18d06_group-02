using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class WalletTransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public WalletTransactionType Type { get; set; }
        public TransactionDirection Direction { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Note { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Currency { get; set; } = "VND";
        public WalletTransactionStatus Status { get; set; }
        public string? Provider { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string? CreatedByAdminId { get; set; }

        // Reference navigations (flattened)
        public string? PayoutBatchCode { get; set; }
    }
}
