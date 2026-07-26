using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/vouchers")]
[ApiController]
public class VouchersController : BaseApiController
{
    private readonly ISellerVoucherService _voucherService;
    private readonly IAdminCategoryService _categoryService;

    public VouchersController(ISellerVoucherService voucherService, IAdminCategoryService categoryService)
    {
        _voucherService = voucherService;
        _categoryService = categoryService;
    }

    // GET /api/seller/vouchers?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] SellerVoucherQueryDto query)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize <= 0) query.PageSize = 10;

        var vouchers = await _voucherService.GetPaginatedVouchersAsync(CurrentUserId!, query);
        var summary = await _voucherService.GetVoucherSummaryAsync(CurrentUserId!);
        var categories = await _categoryService.GetAllCategoriesListAsync();

        return OkResponse(new SellerVoucherListDto
        {
            Vouchers = vouchers,
            Summary = summary,
            Categories = categories
        });
    }

    // GET /api/seller/vouchers/categories — options for the create/edit forms.
    // Declared before the {id:guid} route; "categories" never matches a GUID anyway.
    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
        => OkResponse(await _categoryService.GetAllCategoriesListAsync());

    // GET /api/seller/vouchers/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var voucher = await _voucherService.GetVoucherByIdAsync(CurrentUserId!, id);
        if (voucher == null) return FailResponse("Voucher not found.", 404);
        return OkResponse(voucher);
    }

    // POST /api/seller/vouchers
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SellerCreateVoucherDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        if (dto.DiscountType == "Fixed") dto.MaxDiscount = null;

        try
        {
            var (success, error) = await _voucherService.CreateVoucherAsync(CurrentUserId!, dto);
            if (!success) return VoucherFailure(error, "Failed to create voucher.");
        }
        catch (DbUpdateException)
        {
            return FailResponse(
                "Voucher creation conflicted with another request. Reload and try again.",
                409);
        }

        return OkResponse("Voucher created.");
    }

    // PUT /api/seller/vouchers/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SellerUpdateVoucherDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        if (dto.DiscountType == "Fixed") dto.MaxDiscount = null;

        try
        {
            var (success, error) = await _voucherService.UpdateVoucherAsync(CurrentUserId!, id, dto);
            if (!success) return VoucherFailure(error, "Failed to update voucher.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return FailResponse(
                "The voucher was changed by another request. Reload and try again.",
                409);
        }
        catch (DbUpdateException)
        {
            return FailResponse(
                "Voucher update conflicted with another request. Reload and try again.",
                409);
        }

        return OkResponse("Voucher updated.");
    }

    // PATCH /api/seller/vouchers/{id}/toggle-status
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            var (success, error) = await _voucherService.ToggleVoucherStatusAsync(CurrentUserId!, id);
            if (!success) return VoucherFailure(error, "Failed to update voucher status.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return FailResponse(
                "The voucher was changed by another request. Reload and try again.",
                409);
        }
        return OkResponse("Voucher status updated.");
    }

    private IActionResult VoucherFailure(string? error, string fallback)
    {
        var message = error ?? fallback;
        var statusCode =
            message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                ? 404
                : message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                  message.Contains("cannot be lower", StringComparison.OrdinalIgnoreCase)
                    ? 409
                    : 400;
        return FailResponse(message, statusCode);
    }
}
