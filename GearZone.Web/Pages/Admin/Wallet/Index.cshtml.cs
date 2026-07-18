using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Admin.Wallet
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public WalletSummaryDto Summary { get; set; } = new();
        public PagedResult<WalletTransactionDto> Transactions { get; set; } = new();
        public List<WalletTransactionDto> CashFlowHistory { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public WalletTransactionQuery Query { get; set; } = new();

        [BindProperty]
        public TopupWalletDto TopupInput { get; set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            await LoadDataAsync(ct);
        }

        public async Task<IActionResult> OnPostTopupAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                // Reload page data on validation failure
                await LoadDataAsync(ct);
                return Page();
            }

            var result = await _api.PostAsync("/api/admin/wallet/top-up", TopupInput, ct);
            if (!result.Success)
            {
                var error = result.FirstError ?? "Failed to record wallet top-up.";
                ModelState.AddModelError(string.Empty, error);
                TempData["ErrorMessage"] = error;
                await LoadDataAsync(ct);
                return Page();
            }

            TempData["SuccessMessage"] = $"Top-up {TopupInput.Amount:N0} VND recorded successfully with status Completed.";
            return RedirectToPage();
        }

        private async Task LoadDataAsync(CancellationToken ct)
        {
            var data = await _api.GetAsync<AdminWalletResponseDto>(
                $"/api/admin/wallet{ApiQueryStringBuilder.Build(Query)}", ct);
            if (data is not null)
            {
                Summary = data.Summary;
                Transactions = data.Transactions;
                CashFlowHistory = data.CashFlow;
            }
        }
    }
}
