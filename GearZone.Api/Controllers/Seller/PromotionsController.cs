using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Promotions.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/promotions")]
[ApiController]
public class PromotionsController : BaseApiController
{
    private readonly ISellerPromotionService _promotions;

    public PromotionsController(ISellerPromotionService promotions)
    {
        _promotions = promotions;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] SellerPromotionQueryDto query,
        CancellationToken ct)
    {
        return OkResponse(await _promotions.GetListAsync(CurrentUserId!, query, ct));
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        return OkResponse(await _promotions.GetProductsAsync(
            CurrentUserId!, search, pageNumber, pageSize, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var campaign = await _promotions.GetByIdAsync(CurrentUserId!, id, ct);
        return campaign == null
            ? FailResponse("Promotion campaign not found.", 404)
            : OkResponse(campaign);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] PromotionCampaignInputDto input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationFailResponse();
        }

        var result = await _promotions.CreateAsync(CurrentUserId!, input, ct);
        if (!result.Success)
        {
            return FailResponse(
                result.Error ?? "Could not create promotion campaign.",
                result.Conflict ? 409 : 400);
        }

        return OkResponse("Promotion campaign created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] PromotionCampaignInputDto input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationFailResponse();
        }

        (bool Success, string? Error, bool Conflict) result;
        try
        {
            result = await _promotions.UpdateAsync(CurrentUserId!, id, input, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return FailResponse(
                "The promotion campaign was changed by another request. Reload and try again.",
                409);
        }
        if (!result.Success)
        {
            var statusCode = result.Conflict
                ? 409
                : result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                    ? 404
                    : 400;
            return FailResponse(
                result.Error ?? "Could not update promotion campaign.",
                statusCode);
        }

        return OkResponse("Promotion campaign updated.");
    }

    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        (bool Success, string? Error, bool Conflict) result;
        try
        {
            result = await _promotions.ToggleStatusAsync(CurrentUserId!, id, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return FailResponse(
                "The promotion campaign was changed by another request. Reload and try again.",
                409);
        }
        if (!result.Success)
        {
            var statusCode = result.Conflict
                ? 409
                : result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                    ? 404
                    : 400;
            return FailResponse(
                result.Error ?? "Could not update promotion status.",
                statusCode);
        }

        return OkResponse("Promotion status updated.");
    }
}
