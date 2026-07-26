using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GearZone.Api.Auditing;
using GearZone.Application.Features.Admin;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/stores")]
[ApiController]
public class StoreManagementController : BaseApiController
{
    private readonly IAdminStoreService _storeService;

    public StoreManagementController(IAdminStoreService storeService)
    {
        _storeService = storeService;
    }

    // GET /api/admin/stores?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] StoreApplicationQueryDto query)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        query.ExcludeStatuses = new List<StoreStatus> { StoreStatus.Draft, StoreStatus.Pending, StoreStatus.Rejected };

        var stores = await _storeService.GetStoreApplicationsAsync(query);
        return OkResponse(stores);
    }

    // GET /api/admin/stores/stats
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var stats = await _storeService.GetStoreApplicationStatsAsync();
        return OkResponse(stats);
    }

    // GET /api/admin/stores/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var store = await _storeService.GetStoreApplicationByIdAsync(id);
        if (store == null) return FailResponse("Store not found.", 404);
        return OkResponse(store);
    }

    // POST /api/admin/stores/{id}/change-status
    [HttpPost("{id:guid}/change-status")]
    [AdminAuditAction(AdminAuditActions.StoreStatusChanged, AdminAuditModules.Stores, AdminAuditRiskLevel.High, EntityType = "Store", ReasonArgumentName = "request")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStoreStatusRequest request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        if ((request.Status == StoreStatus.Locked || request.Status == StoreStatus.Rejected)
            && string.IsNullOrWhiteSpace(request.Reason))
            return FailResponse("A reason is required when locking or rejecting a store.");

        if (!string.IsNullOrEmpty(request.Reason) && request.Reason.Length > 500)
            return FailResponse("Reason must not exceed 500 characters.");

        var ok = await _storeService.UpdateStoreStatusAsync(id, request.Status, request.Reason);
        return ok ? OkResponse("Store status updated.") : FailResponse("Failed to update store status.");
    }
}

public class ChangeStoreStatusRequest
{
    public StoreStatus Status { get; set; }
    public string? Reason { get; set; }
}
