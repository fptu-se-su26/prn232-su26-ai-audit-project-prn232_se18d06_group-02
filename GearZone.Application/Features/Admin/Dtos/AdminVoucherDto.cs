using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;
using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminVoucherDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public VoucherType Type { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public int MaxUsagePerUser { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public VoucherStatus Status { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; } // We might need this for the UI
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminVoucherQueryDto : PaginationRequest
    {
        public string? Search { get; set; }
        public VoucherStatus? Status { get; set; }
        public VoucherScope? Scope { get; set; }
        public VoucherType? VoucherType { get; set; }
        public DiscountType? DiscountType { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AdminVoucherSummaryDto
    {
        public int TotalVouchers { get; set; }
        public int ActiveToday { get; set; }
        public decimal RedemptionRate { get; set; }
        public decimal TotalSavedAmount { get; set; }
    }
}
