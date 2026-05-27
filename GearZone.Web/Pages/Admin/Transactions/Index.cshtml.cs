using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using System.Threading.Tasks;
using GearZone.Application.Common.Models; // Keep this as PagedResult is used
using GearZone.Application.Features.Admin.Dtos; // Keep this as AdminPlatformTransactionSummaryDto and PlatformTransactionDto are used

namespace GearZone.Web.Pages.Admin.Transactions
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminPlatformService _platformService;

        public IndexModel(IAdminPlatformService platformService)
        {
            _platformService = platformService;
        }

        [BindProperty(SupportsGet = true)]
        public PlatformTransactionQuery Query { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        public PagedResult<PlatformTransactionDto> Transactions { get; set; }
        public AdminPlatformTransactionSummaryDto Summary { get; set; }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(DateRangeShortcut))
            {
                var today = System.DateTime.UtcNow.Date;
                switch (DateRangeShortcut.ToLower())
                {
                    case "today":
                        Query.StartDate = today;
                        Query.EndDate = today;
                        break;
                    case "week":
                        Query.StartDate = today.AddDays(-7);
                        Query.EndDate = today;
                        break;
                    case "month":
                        Query.StartDate = today.AddDays(-30);
                        Query.EndDate = today;
                        break;
                    case "custom":
                        if (!string.IsNullOrEmpty(DateRange))
                        {
                            var dates = DateRange.Split(" to ");
                            if (dates.Length == 2)
                            {
                                if (System.DateTime.TryParse(dates[0], out var start)) Query.StartDate = start;
                                if (System.DateTime.TryParse(dates[1], out var end)) Query.EndDate = end;
                            }
                            else if (dates.Length == 1)
                            {
                                if (System.DateTime.TryParse(dates[0], out var start))
                                {
                                    Query.StartDate = start;
                                    Query.EndDate = start;
                                }
                            }
                        }
                        break;
                }
            }

            Transactions = await _platformService.GetTransactionsAsync(Query);
            Summary = await _platformService.GetTransactionSummaryAsync(Query);
        }
    }
}
