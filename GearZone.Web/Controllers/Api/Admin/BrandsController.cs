using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/brands")]
[ApiController]
public class BrandsController : BaseApiController
{
    private readonly IAdminBrandService _brandService;

    public BrandsController(IAdminBrandService brandService)
    {
        _brandService = brandService;
    }

    // GET /api/admin/brands?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AdminBrandQueryDto query)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1) query.PageSize = 10;

        var brands = await _brandService.GetBrandsAsync(query);
        var stats = await _brandService.GetBrandStatsAsync();
        return OkResponse(new { brands, stats });
    }

    // GET /api/admin/brands/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var brand = await _brandService.GetBrandByIdAsync(id);
        if (brand == null) return FailResponse("Brand not found.", 404);
        return OkResponse(brand);
    }

    // GET /api/admin/brands/all
    [HttpGet("all")]
    public async Task<IActionResult> All()
    {
        var brands = await _brandService.GetAllBrandsListAsync();
        return OkResponse(brands);
    }

    // POST /api/admin/brands
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBrandDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        var ok = await _brandService.CreateBrandAsync(dto);
        return ok ? OkResponse("Brand created.") : FailResponse("Failed to create brand.");
    }

    // PUT /api/admin/brands
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] EditBrandDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        var ok = await _brandService.UpdateBrandAsync(dto);
        return ok ? OkResponse("Brand updated.") : FailResponse("Failed to update brand.");
    }

    // POST /api/admin/brands/{id}/approve
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var ok = await _brandService.ApproveBrandAsync(id);
        return ok ? OkResponse("Brand approved.") : FailResponse("Failed to approve brand.");
    }

    // POST /api/admin/brands/{id}/reject
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var ok = await _brandService.RejectBrandAsync(id);
        return ok ? OkResponse("Brand rejected.") : FailResponse("Failed to reject brand.");
    }

    // DELETE /api/admin/brands/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _brandService.DeleteBrandAsync(id);
        return ok ? OkResponse("Brand deleted.") : FailResponse("Failed to delete brand.");
    }
}
