using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin.StoreApplications
{
    [Authorize(Roles = "Super Admin")]
    public class DetailModel : PageModel
    {
        private const int MaxReasonLength = 500;

        private readonly IApiClient _api;

        public DetailModel(IApiClient api)
        {
            _api = api;
        }

        public StoreApplicationDto? StoreApplication { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            StoreApplication = await _api.GetAsync<StoreApplicationDto>($"/api/admin/store-applications/{id}", ct);

            if (StoreApplication == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/store-applications/{id}/approve", ct);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = "Failed to approve store application.";
                return RedirectToPage(new { id });
            }
            TempData["SuccessMessage"] = "Store application has been successfully approved.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id, string rejectReason, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(rejectReason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToPage(new { id });
            }

            var normalizedReason = rejectReason.Trim();
            if (normalizedReason.Length > MaxReasonLength)
            {
                TempData["ErrorMessage"] = $"Rejection reason cannot exceed {MaxReasonLength} characters.";
                return RedirectToPage(new { id });
            }

            var result = await _api.PostAsync(
                $"/api/admin/store-applications/{id}/reject", new { reason = normalizedReason }, ct);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = "Failed to reject store application.";
                return RedirectToPage(new { id });
            }
            TempData["SuccessMessage"] = "Store application has been rejected.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRequestInfoAsync(Guid id, string informNote, CancellationToken ct)
        {
            var result = await _api.PostAsync(
                $"/api/admin/store-applications/{id}/request-info", new { note = informNote }, ct);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = "Failed to send request info.";
                return RedirectToPage(new { id });
            }
            TempData["SuccessMessage"] = "Information request has been successfully sent to the seller.";
            return RedirectToPage(new { id });
        }
    }
}
