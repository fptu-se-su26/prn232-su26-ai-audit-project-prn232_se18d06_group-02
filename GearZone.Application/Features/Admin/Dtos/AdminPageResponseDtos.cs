using System.ComponentModel.DataAnnotations;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.ViewModels;

namespace GearZone.Application.Features.Admin.Dtos;

public class AdminUserListResponseDto
{
    public PagedResult<UserViewModel> Users { get; set; } = new();
    public UserStatsDto Stats { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}

public class AdminOrderListResponseDto
{
    public PagedResult<AdminOrderDto> Orders { get; set; } = new();
    public AdminOrderStatsDto Stats { get; set; } = new();
}

public class AdminProductListResponseDto
{
    public PagedResult<AdminProductDto> Products { get; set; } = new();
    public AdminProductStatsDto Stats { get; set; } = new();
}

public class AdminProductMetadataDto
{
    public List<CategoryDto> Categories { get; set; } = new();
    public List<StoreApplicationDto> Stores { get; set; } = new();
    public List<AdminBrandDto> Brands { get; set; } = new();
}

public class AdminBrandListResponseDto
{
    public PagedResult<AdminBrandDto> Brands { get; set; } = new();
    public AdminBrandStatsDto Stats { get; set; } = new();
}

public class AdminVoucherListResponseDto
{
    public PagedResult<AdminVoucherDto> Vouchers { get; set; } = new();
    public AdminVoucherSummaryDto Summary { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug is required")]
    [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
    public string Slug { get; set; } = string.Empty;

    public int? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CategoryCreatedDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public class AdminWalletResponseDto
{
    public WalletSummaryDto Summary { get; set; } = new();
    public PagedResult<WalletTransactionDto> Transactions { get; set; } = new();
    public List<WalletTransactionDto> CashFlow { get; set; } = new();
}

public class AdminSettingsResponseDto
{
    public Dictionary<string, string> Settings { get; set; } = new();
    public string LastSynced { get; set; } = "Never";
}

public class AdminPayoutListResponseDto
{
    public PagedResult<AdminPayoutTransactionDto> Transactions { get; set; } = new();
    public AdminPayoutTransactionSummaryDto Summary { get; set; } = new();
}

public class AdminSellerPayableResponseDto
{
    public List<AdminSellerPayableSummaryDto> Summary { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class PayoutBatchCreatedDto
{
    public string BatchCode { get; set; } = string.Empty;
}
