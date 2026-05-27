using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Payouts
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminPayoutService _payoutService;

        public IndexModel(IAdminPayoutService payoutService)
        {
            _payoutService = payoutService;
        }

        public PagedResult<AdminPayoutTransactionDto> Transactions { get; set; } = null!;
        public AdminPayoutTransactionSummaryDto Summary { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public PayoutTransactionQueryDto Query { get; set; } = new();

        public async Task OnGetAsync()
        {
            Transactions = await _payoutService.GetPayoutTransactionsAsync(Query);
            Summary = await _payoutService.GetPayoutTransactionSummaryAsync(Query);
        }
    }
}
