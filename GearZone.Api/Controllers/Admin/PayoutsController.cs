using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/payouts")]
[ApiController]
public class PayoutsController : BaseApiController
{
    private readonly IAdminPayoutService _adminPayoutService;
    private readonly IPayoutService _payoutService;

    public PayoutsController(IAdminPayoutService adminPayoutService, IPayoutService payoutService)
    {
        _adminPayoutService = adminPayoutService;
        _payoutService = payoutService;
    }

    // GET /api/admin/payouts?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] PayoutTransactionQueryDto query)
    {
        var transactions = await _adminPayoutService.GetPayoutTransactionsAsync(query);
        var summary = await _adminPayoutService.GetPayoutTransactionSummaryAsync(query);
        return OkResponse(new { transactions, summary });
    }

    // GET /api/admin/payouts/batches?[query]
    [HttpGet("batches")]
    public async Task<IActionResult> Batches([FromQuery] AdminPayoutBatchQueryDto query)
    {
        var batches = await _adminPayoutService.GetPayoutBatchesAsync(query);
        return OkResponse(batches);
    }

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
    public async Task<IActionResult> ApproveBatch(Guid id, CancellationToken ct)
    {
        await _payoutService.ApproveBatchAsync(id, CurrentUserId!, ct);
        return OkResponse("Batch approved.");
    }

    // POST /api/admin/payouts/batches/{id}/hold
    [HttpPost("batches/{id:guid}/hold")]
    public async Task<IActionResult> HoldBatch(Guid id, [FromBody] BatchHoldRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return FailResponse("A reason is required to hold a batch.");

        await _payoutService.HoldBatchAsync(id, request.Reason, ct);
        return OkResponse("Batch put on hold.");
    }

    // POST /api/admin/payouts/batches/{id}/process
    [HttpPost("batches/{id:guid}/process")]
    public async Task<IActionResult> ProcessBatch(Guid id, [FromBody] BatchProcessRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.BatchCode))
            return FailResponse("BatchCode is required.");

        await _payoutService.ProcessPayoutBatchAsync(request.BatchCode, ct);
        return OkResponse("Batch processing started.");
    }

    // POST /api/admin/payouts/batches/{batchId}/transactions/{txId}/retry
    [HttpPost("batches/{batchId:guid}/transactions/{txId:guid}/retry")]
    public async Task<IActionResult> RetryTransaction(Guid batchId, Guid txId, CancellationToken ct)
    {
        await _payoutService.RetryTransactionAsync(txId, ct);
        return OkResponse("Transaction retry queued.");
    }

    // POST /api/admin/payouts/batches/{batchId}/transactions/{txId}/exclude
    [HttpPost("batches/{batchId:guid}/transactions/{txId:guid}/exclude")]
    public async Task<IActionResult> ExcludeTransaction(Guid batchId, Guid txId, [FromBody] ExcludeTransactionRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return FailResponse("A reason is required to exclude a transaction.");

        await _payoutService.ExcludeTransactionAsync(txId, request.Reason, ct);
        return OkResponse("Transaction excluded.");
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
        return OkResponse(new { summary, periodStart = start, periodEnd = end });
    }

    // POST /api/admin/payouts/process-bulk
    [HttpPost("process-bulk")]
    public async Task<IActionResult> ProcessBulk([FromBody] ProcessPayoutRequest request, CancellationToken ct)
    {
        if (request.StoreIds == null || !request.StoreIds.Any())
            return FailResponse("No stores selected.");

        var (start, end) = ResolveSellerSummaryDateRange(request.RangeType, request.CustomStart, request.CustomEnd);
        var batchCode = await _payoutService.GenerateApprovedBatchForStoresAsync(start, end, request.StoreIds, CurrentUserId!, ct);
        return OkResponse(new { batchCode }, "Payout batch generated.");
    }

    // POST /api/admin/payouts/process-single/{storeId}
    [HttpPost("process-single/{storeId:guid}")]
    public async Task<IActionResult> ProcessSingle(Guid storeId, [FromBody] ProcessSinglePayoutRequest request, CancellationToken ct)
    {
        var (start, end) = ResolveSellerSummaryDateRange(request.RangeType, request.CustomStart, request.CustomEnd);
        var batchCode = await _payoutService.GenerateApprovedBatchForStoresAsync(start, end, new[] { storeId }, CurrentUserId!, ct);
        return OkResponse(new { batchCode }, "Payout batch generated.");
    }

    private static (DateTime start, DateTime end) ResolveSellerSummaryDateRange(
        string? rangeType, DateTime? customStart, DateTime? customEnd)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return rangeType?.ToLower() switch
        {
            "this-week" => (today.AddDays(-(int)today.DayOfWeek + 1), today.AddDays(1).AddTicks(-1)),
            "last-week" => (today.AddDays(-(int)today.DayOfWeek - 6), today.AddDays(-(int)today.DayOfWeek + 1).AddTicks(-1)),
            "custom" when customStart.HasValue && customEnd.HasValue =>
                (customStart.Value.ToUniversalTime(), customEnd.Value.ToUniversalTime()),
            _ => (now.AddMonths(-1), now)
        };
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
