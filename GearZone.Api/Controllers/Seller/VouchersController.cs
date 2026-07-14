using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        return OkResponse(new { vouchers, summary, categories });
    }

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

        var (success, error) = await _voucherService.CreateVoucherAsync(CurrentUserId!, dto);
        if (!success) return FailResponse(error ?? "Failed to create voucher.");

        return OkResponse("Voucher created.");
    }

    // PUT /api/seller/vouchers/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SellerUpdateVoucherDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        if (dto.DiscountType == "Fixed") dto.MaxDiscount = null;

        var (success, error) = await _voucherService.UpdateVoucherAsync(CurrentUserId!, id, dto);
        if (!success) return FailResponse(error ?? "Failed to update voucher.");

        return OkResponse("Voucher updated.");
    }

    // PATCH /api/seller/vouchers/{id}/toggle-status
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var (success, error) = await _voucherService.ToggleVoucherStatusAsync(CurrentUserId!, id);
        if (!success) return FailResponse(error ?? "Failed to update voucher status.");
        return OkResponse("Voucher status updated.");
    }
}
