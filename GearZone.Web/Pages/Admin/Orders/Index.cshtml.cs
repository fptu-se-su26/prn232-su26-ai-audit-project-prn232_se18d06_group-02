using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin.Orders
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public AdminOrderQueryDto Query { get; set; } = new AdminOrderQueryDto();

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        public PagedResult<AdminOrderDto> Orders { get; set; } = new PagedResult<AdminOrderDto>();
        public AdminOrderStatsDto Stats { get; set; } = new AdminOrderStatsDto();

        public async Task OnGetAsync(CancellationToken ct)
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
                        if (!string.IsNullOrEmpty(Query.DateRange))
                        {
                            var dates = Query.DateRange.Split(" to ");
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

            Query.PageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;
            Query.PageSize = Query.PageSize < 1 ? 10 : Query.PageSize;

            var data = await _api.GetAsync<AdminOrderListResponseDto>(
                $"/api/admin/orders{ApiQueryStringBuilder.Build(Query)}", ct);
            if (data is not null)
            {
                Stats = data.Stats;
                Orders = data.Orders;
            }
        }
    }
}
