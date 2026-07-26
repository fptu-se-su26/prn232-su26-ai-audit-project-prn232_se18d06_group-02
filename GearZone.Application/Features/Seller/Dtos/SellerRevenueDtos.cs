using System;
using System.Text.Json.Serialization;
using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Seller.Dtos
{
    /// <summary>Filter/sort/paging options for the seller payout (revenue) listing.</summary>
    public class SellerRevenueQueryDto
    {
        public string? SearchTerm { get; set; }
        public PayoutTransactionStatus? Status { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        /// <summary>today | week | month | custom (custom reads <see cref="DateRange"/>).</summary>
        public string? DateRangeShortcut { get; set; }
        /// <summary>"yyyy-MM-dd to yyyy-MM-dd" when DateRangeShortcut is "custom".</summary>
        public string? DateRange { get; set; }
        public string SortBy { get; set; } = "date";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
    }

    public class SellerPayoutSummaryDto
    {
        public int TotalTransactions { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class SellerPayoutTransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionCode { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;

        // Serialized as a string (e.g. "Success") so JS clients keep reading a name,
        // while typed .NET clients still deserialize straight back into the enum.
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PayoutTransactionStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class SellerRevenueDto
    {
        public bool HasStore { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public SellerPayoutSummaryDto Summary { get; set; } = new();
        public PagedResult<SellerPayoutTransactionDto> Transactions { get; set; } = new();
    }
}
