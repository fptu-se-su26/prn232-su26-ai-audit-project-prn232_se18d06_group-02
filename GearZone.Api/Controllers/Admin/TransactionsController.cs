using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/transactions")]
[ApiController]
public class TransactionsController : BaseApiController
{
    private readonly IAdminPlatformService _platformService;

    public TransactionsController(IAdminPlatformService platformService)
    {
        _platformService = platformService;
    }

    // GET /api/admin/transactions?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PlatformTransactionQuery query, CancellationToken ct)
    {
        var transactions = await _platformService.GetTransactionsAsync(query, ct);
        return OkResponse(transactions);
    }

    // GET /api/admin/transactions/summary?[query]
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] PlatformTransactionQuery query, CancellationToken ct)
        => OkResponse(await _platformService.GetTransactionSummaryAsync(query, ct));
}
