using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Payouts
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        public PagedResult<AdminPayoutTransactionDto> Transactions { get; set; } = new();
        public AdminPayoutTransactionSummaryDto Summary { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public PayoutTransactionQueryDto Query { get; set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            Query.PageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;
            Query.PageSize = Query.PageSize < 1 ? 10 : Query.PageSize;
            var data = await _api.GetAsync<AdminPayoutListResponseDto>(
                $"/api/admin/payouts{ApiQueryStringBuilder.Build(Query)}", ct);
            if (data is not null)
            {
                Transactions = data.Transactions;
                Summary = data.Summary;
            }
        }
    }
}
