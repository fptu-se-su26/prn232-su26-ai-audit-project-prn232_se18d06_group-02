using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/store-applications")]
[ApiController]
public class StoreApplicationsController : BaseApiController
{
    private readonly IAdminStoreService _storeService;
    private const int MaxReasonLength = 500;

    public StoreApplicationsController(IAdminStoreService storeService)
    {
        _storeService = storeService;
    }

    // GET /api/admin/store-applications?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] StoreApplicationQueryDto query)
    {
        var applications = await _storeService.GetStoreApplicationsAsync(query);
        return OkResponse(applications);
    }

    // GET /api/admin/store-applications/stats
    [HttpGet("stats")]
    public async Task<IActionResult> Stats()
    {
        var stats = await _storeService.GetStoreApplicationStatsAsync();
        return OkResponse(stats);
    }

    // GET /api/admin/store-applications/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var application = await _storeService.GetStoreApplicationByIdAsync(id);
        if (application == null) return FailResponse("Application not found.", 404);
        return OkResponse(application);
    }

    // POST /api/admin/store-applications/{id}/approve
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await _storeService.UpdateStoreStatusAsync(id, StoreStatus.Approved);
        return ok ? OkResponse("Store application approved.") : FailResponse("Failed to approve application.");
    }

    // POST /api/admin/store-applications/{id}/reject
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectStoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return FailResponse("Rejection reason is required.");

        var reason = request.Reason.Trim();
        if (reason.Length > MaxReasonLength)
            return FailResponse($"Reason cannot exceed {MaxReasonLength} characters.");

        var ok = await _storeService.UpdateStoreStatusAsync(id, StoreStatus.Rejected, reason);
        return ok ? OkResponse("Store application rejected.") : FailResponse("Failed to reject application.");
    }

    // POST /api/admin/store-applications/{id}/request-info
    [HttpPost("{id:guid}/request-info")]
    public async Task<IActionResult> RequestInfo(Guid id, [FromBody] RequestInfoRequest request)
    {
        var ok = await _storeService.RequestInfoAsync(id, request.Note);
        return ok ? OkResponse("Information request sent.") : FailResponse("Failed to send request.");
    }
}

public class RejectStoreRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class RequestInfoRequest
{
    public string Note { get; set; } = string.Empty;
}
