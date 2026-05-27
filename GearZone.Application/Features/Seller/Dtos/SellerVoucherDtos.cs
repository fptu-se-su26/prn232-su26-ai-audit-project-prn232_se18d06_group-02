using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class SellerVoucherDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int UsageLimit { get; set; }
        public int MaxUsagePerUser { get; set; }
        public int UsedCount { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryIcon { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public VoucherStatus Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SellerVoucherQueryDto : PaginationRequest
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

    public class SellerVoucherSummaryDto
    {
        public int TotalVouchers { get; set; }
        public int ActiveToday { get; set; }
        public decimal RedemptionRate { get; set; }
        public decimal TotalSavedAmount { get; set; }
    }

    public class SellerCreateVoucherDto
    {
        [Required(ErrorMessage = "Voucher name is required")]
        [StringLength(100, ErrorMessage = "Voucher name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Voucher code is required")]
        [StringLength(12, ErrorMessage = "Voucher code cannot exceed 12 characters")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Only letters and numbers are allowed")]
        public string Code { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string Type { get; set; } = "Order";

        [Required]
        public string DiscountType { get; set; } = "Percent"; // Percent or Fixed

        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, 1000000000, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        public decimal? MaxDiscount { get; set; }

        [Required(ErrorMessage = "Minimum spend is required")]
        [Range(0, 1000000000, ErrorMessage = "Minimum spend cannot be negative")]
        public decimal MinOrderAmount { get; set; }

        [Required(ErrorMessage = "Total usage limit is required")]
        [Range(1, 1000000, ErrorMessage = "Usage limit must be at least 1")]
        public int UsageLimit { get; set; }

        [Required(ErrorMessage = "Max usage per user is required")]
        [Range(1, 1000, ErrorMessage = "Max usage per user must be between 1 and 1000")]
        public int MaxUsagePerUser { get; set; } = 1;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "Expiration date is required")]
        public DateTime EndAt { get; set; }

        public int? CategoryId { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class SellerUpdateVoucherDto : SellerCreateVoucherDto
    {
    }
}
