using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/dashboard")]
[ApiController]
public class DashboardController : BaseApiController
{
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET /api/admin/dashboard?[query]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DashboardQuery query)
    {
        var data = await _dashboardService.GetDashboardDataAsync(query);
        return OkResponse(data);
    }
}
