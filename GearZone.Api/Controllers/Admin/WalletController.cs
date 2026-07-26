using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GearZone.Api.Auditing;
using GearZone.Application.Features.Admin;
using GearZone.Domain.Enums;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/wallet")]
[ApiController]
public class WalletController : BaseApiController
{
    private readonly IAdminWalletService _walletService;

    public WalletController(IAdminWalletService walletService)
    {
        _walletService = walletService;
    }

    // GET /api/admin/wallet?[query]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] WalletTransactionQuery query)
    {
        var summary = await _walletService.GetWalletSummaryAsync();
        var transactions = await _walletService.GetTransactionsAsync(query);
        var cashFlow = await _walletService.GetCashFlowHistoryAsync(recentMonths: 6);
        return OkResponse(new AdminWalletResponseDto
        {
            Summary = summary,
            Transactions = transactions,
            CashFlow = cashFlow
        });
    }

    // POST /api/admin/wallet/top-up
    [HttpPost("top-up")]
    [AdminAuditAction(AdminAuditActions.WalletToppedUp, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "WalletTransaction")]
    public async Task<IActionResult> TopUp([FromBody] TopupWalletDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        await _walletService.RecordTopupAsync(dto, CurrentUserId!);
        return OkResponse($"Top-up {dto.Amount:N0} VND recorded successfully.");
    }
}
