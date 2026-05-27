using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Infrastructure.Jobs;
using Hangfire;
using System.Security.Claims;

namespace GearZone.Web.Pages.Admin.PayoutBatches
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminPayoutService _payoutService;
        private readonly IPayoutService _payoutActionService;
        private readonly IBackgroundJobClient _backgroundJobs;

        public IndexModel(
            IAdminPayoutService payoutService,
            IPayoutService payoutActionService,
            IBackgroundJobClient backgroundJobs)
        {
            _payoutService = payoutService;
            _payoutActionService = payoutActionService;
            _backgroundJobs = backgroundJobs;
        }

        [BindProperty(SupportsGet = true)]
        public AdminPayoutBatchQueryDto Query { get; set; } = new AdminPayoutBatchQueryDto();

        public PagedResult<AdminPayoutBatchDto> Batches { get; set; } = new PagedResult<AdminPayoutBatchDto>();
        public AdminPayoutBatchSummaryDto Summary { get; set; } = new AdminPayoutBatchSummaryDto();

        public async Task OnGetAsync()
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

            Batches = await _payoutService.GetPayoutBatchesAsync(Query);
            Summary = await _payoutService.GetPayoutSummaryAsync(Query);
        }

        public async Task<IActionResult> OnPostApproveBatchAsync(Guid id)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            try
            {
                await _payoutActionService.ApproveBatchAsync(id, adminId);
                TempData["SuccessMessage"] = "Batch approved successfully. You can now process it.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to approve batch: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostHoldBatchAsync(Guid id, string reason)
        {
            try
            {
                await _payoutActionService.HoldBatchAsync(id, reason);
                TempData["SuccessMessage"] = "Batch placed on hold.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to hold batch: {ex.Message}";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostProcessBatchAsync(Guid id, string batchCode)
        {
            try
            {
                // Enqueue as a Hangfire background job — the PayOS batch API call is long-running
                // and needs automatic retry on failure (configured in PayoutBatchJob)
                _backgroundJobs.Enqueue<PayoutBatchJob>(
                    job => job.ProcessApprovedBatchAsync(batchCode));

                TempData["SuccessMessage"] = $"Batch '{batchCode}' has been queued for processing. Check Hangfire dashboard for progress.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to queue batch processing: {ex.Message}";
            }
            return RedirectToPage();
        }
    }
}
