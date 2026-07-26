using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin.PayoutBatches
{
    [Authorize(Roles = "Super Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IApiClient _api;

        public DetailsModel(IApiClient api)
        {
            _api = api;
        }

        public AdminPayoutBatchDto? Batch { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            Batch = await _api.GetAsync<AdminPayoutBatchDto>($"/api/admin/payouts/batches/{id}", ct);
            if (Batch == null) return NotFound();
            return Page();
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
                TempData["ErrorMessage"] = $"Failed to approve: {result.FirstError}";
            }
            return RedirectToPage(new { id });
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
                TempData["ErrorMessage"] = $"Failed to hold: {result.FirstError}";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostProcessBatchAsync(Guid id, string batchCode, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/payouts/batches/{id}/process", new { batchCode }, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Batch '{batchCode}' has been queued for processing. Status will update automatically when done.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to queue batch processing: {result.FirstError}";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRetryTransactionAsync(Guid id, Guid txId, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/payouts/batches/{id}/transactions/{txId}/retry", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Transaction retry initiated.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to retry: {result.FirstError}";
            }
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostExcludeTransactionAsync(
            Guid id, Guid txId, string reason, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/payouts/batches/{id}/transactions/{txId}/exclude", new { reason }, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Transaction excluded.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to exclude: {result.FirstError}";
            }
            return RedirectToPage(new { id });
        }
    }
}
