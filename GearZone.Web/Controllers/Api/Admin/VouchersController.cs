using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/vouchers")]
[ApiController]
public class VouchersController : BaseApiController
{
    private readonly IAdminVoucherService _voucherService;
    private readonly IAdminCategoryService _categoryService;

    public VouchersController(IAdminVoucherService voucherService, IAdminCategoryService categoryService)
    {
        _voucherService = voucherService;
        _categoryService = categoryService;
    }

    // GET /api/admin/vouchers?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AdminVoucherQueryDto query)
    {
        var vouchers = await _voucherService.GetPaginatedVouchersAsync(query);
        var categories = await _categoryService.GetAllCategoriesListAsync();
        return OkResponse(new { vouchers, categories });
    }

    // GET /api/admin/vouchers/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var voucher = await _voucherService.GetVoucherByIdAsync(id);
        if (voucher == null) return FailResponse("Voucher not found.", 404);
        return OkResponse(voucher);
    }

    // POST /api/admin/vouchers
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVoucherDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        if (dto.DiscountType == "Fixed") dto.MaxDiscount = null;
        if (dto.EndAt <= dto.StartAt) return FailResponse("Expiration date must be after start date.");

        var ok = await _voucherService.CreateVoucherAsync(dto);
        return ok ? OkResponse("Voucher created.") : FailResponse("Failed to create voucher.");
    }

    // PUT /api/admin/vouchers/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var ok = await _voucherService.UpdateVoucherAsync(id, dto);
        return ok ? OkResponse("Voucher updated.") : FailResponse("Failed to update voucher.");
    }

    // PATCH /api/admin/vouchers/{id}/toggle-status
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var ok = await _voucherService.ToggleVoucherStatusAsync(id);
        return ok ? OkResponse("Voucher status toggled.") : FailResponse("Failed to toggle voucher status.");
    }
}
