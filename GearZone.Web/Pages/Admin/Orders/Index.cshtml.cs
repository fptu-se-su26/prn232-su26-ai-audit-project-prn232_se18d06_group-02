using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Web.Pages.Admin.Orders
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminOrderService _orderService;

        public IndexModel(IAdminOrderService orderService)
        {
            _orderService = orderService;
        }

        [BindProperty(SupportsGet = true)]
        public AdminOrderQueryDto Query { get; set; } = new AdminOrderQueryDto();

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        public PagedResult<AdminOrderDto> Orders { get; set; } = new PagedResult<AdminOrderDto>();
        public AdminOrderStatsDto Stats { get; set; } = new AdminOrderStatsDto();

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

            Stats = await _orderService.GetOrderStatsAsync();
            Orders = await _orderService.GetOrdersAsync(Query);
        }
    }
}
