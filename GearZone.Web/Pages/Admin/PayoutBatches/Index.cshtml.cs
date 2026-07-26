using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin.PayoutBatches
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public AdminPayoutBatchQueryDto Query { get; set; } = new AdminPayoutBatchQueryDto();

        public PagedResult<AdminPayoutBatchDto> Batches { get; set; } = new PagedResult<AdminPayoutBatchDto>();
        public AdminPayoutBatchSummaryDto Summary { get; set; } = new AdminPayoutBatchSummaryDto();

        public async Task OnGetAsync(CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(Query.DateRange) && Query.DateRange.ToLower() != "custom")
            {
                var today = DateTime.UtcNow.Date;
                switch (Query.DateRange.ToLower())
                {
                    case "today": Query.StartDate = today; Query.EndDate = today.AddDays(1).AddSeconds(-1); break;
                    case "week": Query.StartDate = today.AddDays(-7); Query.EndDate = today.AddDays(1).AddSeconds(-1); break;
                    case "month": Query.StartDate = today.AddDays(-30); Query.EndDate = today.AddDays(1).AddSeconds(-1); break;
                    case "year": Query.StartDate = today.AddDays(-365); Query.EndDate = today.AddDays(1).AddSeconds(-1); break;
                }
            }

            Query.PageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;
            Query.PageSize = Query.PageSize < 1 ? 10 : Query.PageSize;
            var queryString = ApiQueryStringBuilder.Build(Query);
            var batchesTask = _api.GetAsync<PagedResult<AdminPayoutBatchDto>>(
                $"/api/admin/payouts/batches{queryString}", ct);
            var summaryTask = _api.GetAsync<AdminPayoutBatchSummaryDto>(
                $"/api/admin/payouts/batches/summary{queryString}", ct);
            await Task.WhenAll(batchesTask, summaryTask);
            Batches = await batchesTask ?? Batches;
            Summary = await summaryTask ?? Summary;
        }

        public async Task<IActionResult> OnPostApproveBatchAsync(Guid id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/payouts/batches/{id}/approve", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Batch approved successfully. You can now process it.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to approve batch: {result.FirstError}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostHoldBatchAsync(Guid id, string reason, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/payouts/batches/{id}/hold", new { reason }, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Batch placed on hold.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to hold batch: {result.FirstError}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostProcessBatchAsync(Guid id, string batchCode, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/payouts/batches/{id}/process", new { batchCode }, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Batch '{batchCode}' has been queued for processing. Check Hangfire dashboard for progress.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to queue batch processing: {result.FirstError}";
            }
            return RedirectToPage();
        }
    }
}
