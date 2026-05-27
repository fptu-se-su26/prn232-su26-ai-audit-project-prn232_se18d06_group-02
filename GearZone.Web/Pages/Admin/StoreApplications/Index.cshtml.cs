using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.StoreApplications
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminStoreService _adminStoreService;

        public IndexModel(IAdminStoreService adminStoreService)
        {
            _adminStoreService = adminStoreService;
        }

        [BindProperty(SupportsGet = true)]
        public StoreApplicationQueryDto Query { get; set; } = new StoreApplicationQueryDto();

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        public PagedResult<StoreApplicationDto> StoreApplications { get; set; } = new() { Items = new(), TotalCount = 0, PageNumber = 1, PageSize = 10 };
        public StoreApplicationStatsDto Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(DateRangeShortcut))
            {
                var today = DateTime.UtcNow.Date;
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
                                if (DateTime.TryParse(dates[0], out var start)) Query.StartDate = start;
                                if (DateTime.TryParse(dates[1], out var end)) Query.EndDate = end;
                            }
                            else if (dates.Length == 1)
                            {
                                if (DateTime.TryParse(dates[0], out var start))
                                {
                                    Query.StartDate = start;
                                    Query.EndDate = start;
                                }
                            }
                        }
                        break;
                }
            }

            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            StoreApplications = await _adminStoreService.GetStoreApplicationsAsync(Query);
            Stats = await _adminStoreService.GetStoreApplicationStatsAsync();
        }
    }
}
