using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Infrastructure.Jobs;
using Hangfire;
using System.Security.Claims;

namespace GearZone.Web.Pages.Admin.PayoutBatches
{
    [Authorize(Roles = "Super Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IAdminPayoutService _payoutService;
        private readonly IPayoutService _payoutActionService;
        private readonly IBackgroundJobClient _backgroundJobs;

        public DetailsModel(
            IAdminPayoutService payoutService,
            IPayoutService payoutActionService,
            IBackgroundJobClient backgroundJobs)
        {
            _payoutService = payoutService;
            _payoutActionService = payoutActionService;
            _backgroundJobs = backgroundJobs;
        }

        public AdminPayoutBatchDto? Batch { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Batch = await _payoutService.GetPayoutBatchDetailAsync(id);
            if (Batch == null) return NotFound();
            return Page();
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
                TempData["ErrorMessage"] = $"Failed to approve: {ex.Message}";
            }
            return RedirectToPage(new { id });
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
                TempData["ErrorMessage"] = $"Failed to hold: {ex.Message}";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostProcessBatchAsync(Guid id, string batchCode)
        {
            try
            {
                // Enqueue as a Hangfire background job — PayOS batch API is long-running
                // and has automatic retry on failure (2 retries, configured in PayoutBatchJob)
                _backgroundJobs.Enqueue<PayoutBatchJob>(
                    job => job.ProcessApprovedBatchAsync(batchCode));

                TempData["SuccessMessage"] = $"Batch '{batchCode}' has been queued for processing. Status will update automatically when done.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to queue batch processing: {ex.Message}";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRetryTransactionAsync(Guid id, Guid txId)
        {
            try
            {
                await _payoutActionService.RetryTransactionAsync(txId);
                TempData["SuccessMessage"] = "Transaction retry initiated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to retry: {ex.Message}";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostExcludeTransactionAsync(Guid id, Guid txId, string reason)
        {
            try
            {
                await _payoutActionService.ExcludeTransactionAsync(txId, reason);
                TempData["SuccessMessage"] = "Transaction excluded.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to exclude: {ex.Message}";
            }
            return RedirectToPage(new { id });
        }
    }
}
