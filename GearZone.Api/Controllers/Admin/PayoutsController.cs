using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Api.Controllers;
using GearZone.Infrastructure.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GearZone.Api.Auditing;
using GearZone.Application.Features.Admin;
using GearZone.Domain.Enums;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/payouts")]
[ApiController]
public class PayoutsController : BaseApiController
{
    private readonly IAdminPayoutService _adminPayoutService;
    private readonly IPayoutService _payoutService;
    private readonly IBackgroundJobClient _backgroundJobs;

    public PayoutsController(
        IAdminPayoutService adminPayoutService,
        IPayoutService payoutService,
        IBackgroundJobClient backgroundJobs)
    {
        _adminPayoutService = adminPayoutService;
        _payoutService = payoutService;
        _backgroundJobs = backgroundJobs;
    }

    // GET /api/admin/payouts?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PayoutTransactionQueryDto query)
    {
        var transactions = await _adminPayoutService.GetPayoutTransactionsAsync(query);
        var summary = await _adminPayoutService.GetPayoutTransactionSummaryAsync(query);
        return OkResponse(new AdminPayoutListResponseDto { Transactions = transactions, Summary = summary });
    }

    // GET /api/admin/payouts/batches?[query]
    [HttpGet("batches")]
    public async Task<IActionResult> Batches([FromQuery] AdminPayoutBatchQueryDto query)
    {
        var batches = await _adminPayoutService.GetPayoutBatchesAsync(query);
        return OkResponse(batches);
    }

    // GET /api/admin/payouts/batches/summary?[query]
    [HttpGet("batches/summary")]
    public async Task<IActionResult> BatchSummary([FromQuery] AdminPayoutBatchQueryDto query)
        => OkResponse(await _adminPayoutService.GetPayoutSummaryAsync(query));

    // GET /api/admin/payouts/batches/{id}
    [HttpGet("batches/{id:guid}")]
    public async Task<IActionResult> BatchDetail(Guid id)
    {
        var batch = await _adminPayoutService.GetPayoutBatchDetailAsync(id);
        if (batch == null) return FailResponse("Batch not found.", 404);
        return OkResponse(batch);
    }

    // POST /api/admin/payouts/batches/{id}/approve
    [HttpPost("batches/{id:guid}/approve")]
    [AdminAuditAction(AdminAuditActions.PayoutBatchApproved, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutBatch")]
    public async Task<IActionResult> ApproveBatch(Guid id, CancellationToken ct)
    {
        try
        {
            await _payoutService.ApproveBatchAsync(id, CurrentUserId!, ct);
            return OkResponse("Batch approved.");
        }
        catch (Exception ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/admin/payouts/batches/{id}/hold
    [HttpPost("batches/{id:guid}/hold")]
    [AdminAuditAction(AdminAuditActions.PayoutBatchHeld, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutBatch", ReasonArgumentName = "request")]
    public async Task<IActionResult> HoldBatch(Guid id, [FromBody] BatchHoldRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return FailResponse("A reason is required to hold a batch.");

        try
        {
            await _payoutService.HoldBatchAsync(id, request.Reason, ct);
            return OkResponse("Batch put on hold.");
        }
        catch (Exception ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/admin/payouts/batches/{id}/process
    [HttpPost("batches/{id:guid}/process")]
    [AdminAuditAction(AdminAuditActions.PayoutProcessQueued, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutBatch", SuccessOutcome = AdminAuditOutcome.Queued)]
    public async Task<IActionResult> ProcessBatch(Guid id, [FromBody] BatchProcessRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.BatchCode))
            return FailResponse("BatchCode is required.");

        var batch = await _adminPayoutService.GetPayoutBatchDetailAsync(id);
        if (batch == null || !string.Equals(batch.BatchCode, request.BatchCode, StringComparison.OrdinalIgnoreCase))
            return FailResponse("Batch not found or batch code does not match.", 404);

        try
        {
            var correlationId = HttpContext.Response.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? HttpContext.TraceIdentifier;
            var actorId = CurrentUserId;
            _backgroundJobs.Enqueue<PayoutBatchJob>(job =>
                job.ProcessApprovedBatchAsync(request.BatchCode, correlationId, actorId));
            return OkResponse("Batch processing queued.");
        }
        catch (Exception ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/admin/payouts/batches/{batchId}/transactions/{txId}/retry
    [HttpPost("batches/{batchId:guid}/transactions/{txId:guid}/retry")]
    [AdminAuditAction(AdminAuditActions.PayoutTransactionRetried, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutTransaction", RouteIdName = "txId", SuccessOutcome = AdminAuditOutcome.Queued)]
    public async Task<IActionResult> RetryTransaction(Guid batchId, Guid txId, CancellationToken ct)
    {
        try
        {
            await _payoutService.RetryTransactionAsync(txId, ct);
            return OkResponse("Transaction retry queued.");
        }
        catch (Exception ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // POST /api/admin/payouts/batches/{batchId}/transactions/{txId}/exclude
    [HttpPost("batches/{batchId:guid}/transactions/{txId:guid}/exclude")]
    [AdminAuditAction(AdminAuditActions.PayoutTransactionExcluded, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutTransaction", RouteIdName = "txId", ReasonArgumentName = "request")]
    public async Task<IActionResult> ExcludeTransaction(Guid batchId, Guid txId, [FromBody] ExcludeTransactionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return FailResponse("A reason is required to exclude a transaction.");

        try
        {
            await _payoutService.ExcludeTransactionAsync(txId, request.Reason, ct);
            return OkResponse("Transaction excluded.");
        }
        catch (Exception ex)
        {
            return FailResponse(ex.Message);
        }
    }

    // GET /api/admin/payouts/transactions/{id}
    [HttpGet("transactions/{id:guid}")]
    public async Task<IActionResult> TransactionDetail(Guid id)
    {
        var detail = await _adminPayoutService.GetPayoutTransactionDetailAsync(id);
        if (detail == null) return FailResponse("Transaction not found.", 404);
        return OkResponse(detail);
    }

    // GET /api/admin/payouts/seller-summary?rangeType=this-week&customStart=&customEnd=
    [HttpGet("seller-summary")]
    public async Task<IActionResult> SellerSummary(
        [FromQuery] string? rangeType,
        [FromQuery] DateTime? customStart,
        [FromQuery] DateTime? customEnd)
    {
        var (start, end) = ResolveSellerSummaryDateRange(rangeType, customStart, customEnd);
        var summary = await _adminPayoutService.GetSellerPayableSummaryAsync(start, end);
        return OkResponse(new AdminSellerPayableResponseDto
        {
            Summary = summary,
            PeriodStart = start,
            PeriodEnd = end
        });
    }

    // POST /api/admin/payouts/process-bulk
    [HttpPost("process-bulk")]
    [AdminAuditAction(AdminAuditActions.PayoutBatchGenerated, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutBatch")]
    public async Task<IActionResult> ProcessBulk([FromBody] ProcessPayoutRequest request, CancellationToken ct)
    {
        if (request.StoreIds == null || !request.StoreIds.Any())
            return FailResponse("No stores selected.");

        var (start, end) = ResolveSellerSummaryDateRange(request.RangeType, request.CustomStart, request.CustomEnd);
        var batchCode = await _payoutService.GenerateApprovedBatchForStoresAsync(start, end, request.StoreIds, CurrentUserId!, ct);
        return OkResponse(new PayoutBatchCreatedDto { BatchCode = batchCode }, "Payout batch generated.");
    }

    // POST /api/admin/payouts/process-single/{storeId}
    [HttpPost("process-single/{storeId:guid}")]
    [AdminAuditAction(AdminAuditActions.PayoutBatchGenerated, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutBatch", RouteIdName = "storeId")]
    public async Task<IActionResult> ProcessSingle(Guid storeId, [FromBody] ProcessSinglePayoutRequest request, CancellationToken ct)
    {
        var (start, end) = ResolveSellerSummaryDateRange(request.RangeType, request.CustomStart, request.CustomEnd);
        var batchCode = await _payoutService.GenerateApprovedBatchForStoresAsync(start, end, new[] { storeId }, CurrentUserId!, ct);
        return OkResponse(new PayoutBatchCreatedDto { BatchCode = batchCode }, "Payout batch generated.");
    }

    // POST /api/admin/payouts/process-generated
    // Used by the Razor seller-payable workflow, which historically waited for
    // processing to finish before showing the final batch result.
    [HttpPost("process-generated")]
    [AdminAuditAction(AdminAuditActions.PayoutProcessSucceeded, AdminAuditModules.Finance, AdminAuditRiskLevel.Critical, EntityType = "PayoutBatch")]
    public async Task<IActionResult> ProcessGenerated([FromBody] BatchProcessRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.BatchCode))
            return FailResponse("BatchCode is required.");

        try
        {
            await _payoutService.ProcessPayoutBatchAsync(request.BatchCode, ct);
            var batches = await _adminPayoutService.GetPayoutBatchesAsync(new AdminPayoutBatchQueryDto
            {
                SearchTerm = request.BatchCode,
                PageNumber = 1,
                PageSize = 5
            });
            var batch = batches.Items.FirstOrDefault(x => x.BatchCode == request.BatchCode);
            return batch is null
                ? FailResponse("Batch was processed but could not be reloaded.")
                : OkResponse(batch, "Payout batch processed.");
        }
        catch (Exception ex)
        {
            return FailResponse(ex.Message);
        }
    }

    private static (DateTime start, DateTime end) ResolveSellerSummaryDateRange(
        string? rangeType, DateTime? customStart, DateTime? customEnd)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var currentWeekStart = StartOfWeek(today, DayOfWeek.Monday);

        return rangeType?.ToLower() switch
        {
            "last-week" => (currentWeekStart.AddDays(-7), currentWeekStart.AddTicks(-1)),
            "custom" => (
                (customStart ?? now.AddDays(-7)).Date,
                (customEnd ?? now).Date.AddDays(1).AddTicks(-1)),
            _ => (currentWeekStart, now)
        };
    }

    private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }
}

public class BatchHoldRequest { public string Reason { get; set; } = string.Empty; }
public class BatchProcessRequest { public string BatchCode { get; set; } = string.Empty; }
public class ExcludeTransactionRequest { public string Reason { get; set; } = string.Empty; }

public class ProcessPayoutRequest
{
    public List<Guid> StoreIds { get; set; } = new();
    public string? RangeType { get; set; }
    public DateTime? CustomStart { get; set; }
    public DateTime? CustomEnd { get; set; }
}

public class ProcessSinglePayoutRequest
{
    public string? RangeType { get; set; }
    public DateTime? CustomStart { get; set; }
    public DateTime? CustomEnd { get; set; }
}
