using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Payouts
{
    [Authorize(Roles = "Super Admin")]
    public class TransactionDetailModel : PageModel
    {
        private readonly IAdminPayoutService _payoutService;

        public TransactionDetailModel(IAdminPayoutService payoutService)
        {
            _payoutService = payoutService;
        }

        public AdminPayoutTransactionDetailDto Transaction { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var tx = await _payoutService.GetPayoutTransactionDetailAsync(id);
            if (tx == null)
            {
                return NotFound();
            }

            Transaction = tx;
            return Page();
        }
    }
}
