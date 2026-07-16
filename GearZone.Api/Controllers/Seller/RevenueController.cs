using GearZone.Api.Controllers;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/revenue")]
[ApiController]
public class RevenueController : BaseApiController
{
    private readonly ISellerRevenueService _revenueService;

    public RevenueController(ISellerRevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    // GET /api/seller/revenue?searchTerm=&status=&minAmount=&maxAmount=
    //     &dateRangeShortcut=&dateRange=&sortBy=&sortDirection=&pageNumber=
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SellerRevenueQueryDto query, CancellationToken ct)
        => OkResponse(await _revenueService.GetRevenueAsync(CurrentUserId!, query, ct));
}
