using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Admin.Wallet
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminWalletService _walletService;

        public IndexModel(IAdminWalletService walletService)
        {
            _walletService = walletService;
        }

        public WalletSummaryDto Summary { get; set; } = new();
        public PagedResult<WalletTransactionDto> Transactions { get; set; } = new();
        public List<WalletTransactionDto> CashFlowHistory { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public WalletTransactionQuery Query { get; set; } = new();

        [BindProperty]
        public TopupWalletDto TopupInput { get; set; } = new();

        public async Task OnGetAsync()
        {
            Summary = await _walletService.GetWalletSummaryAsync();
            Transactions = await _walletService.GetTransactionsAsync(Query);
            CashFlowHistory = await _walletService.GetCashFlowHistoryAsync(recentMonths: 6);
        }

        public async Task<IActionResult> OnPostTopupAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload page data on validation failure
                Summary = await _walletService.GetWalletSummaryAsync();
                Transactions = await _walletService.GetTransactionsAsync(Query);
                CashFlowHistory = await _walletService.GetCashFlowHistoryAsync(recentMonths: 6);
                return Page();
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

            await _walletService.RecordTopupAsync(TopupInput, adminId);

            TempData["SuccessMessage"] = $"Top-up {TopupInput.Amount:N0} VND recorded successfully with status Completed.";
            return RedirectToPage();
        }
    }
}
