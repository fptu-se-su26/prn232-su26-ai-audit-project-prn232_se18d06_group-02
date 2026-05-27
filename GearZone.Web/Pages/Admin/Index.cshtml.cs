using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.Admin
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IAdminDashboardService _dashboardService;

        public IndexModel(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [BindProperty(SupportsGet = true)]
        public DashboardQuery Query { get; set; } = new();

        public AdminDashboardDto DashboardData { get; set; } = new();

        public async Task OnGetAsync()
        {
            DashboardData = await _dashboardService.GetDashboardDataAsync(Query);
        }
    }
}
