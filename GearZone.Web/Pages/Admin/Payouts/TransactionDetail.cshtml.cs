using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Payouts
{
    [Authorize(Roles = "Super Admin")]
    public class TransactionDetailModel : PageModel
    {
        private readonly IApiClient _api;

        public TransactionDetailModel(IApiClient api)
        {
            _api = api;
        }

        public AdminPayoutTransactionDetailDto Transaction { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            var tx = await _api.GetAsync<AdminPayoutTransactionDetailDto>(
                $"/api/admin/payouts/transactions/{id}", ct);
            if (tx == null)
            {
                return NotFound();
            }

            Transaction = tx;
            return Page();
        }
    }
}
