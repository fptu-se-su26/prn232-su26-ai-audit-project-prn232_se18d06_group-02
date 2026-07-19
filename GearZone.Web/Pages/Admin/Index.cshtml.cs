using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin
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
        public DashboardQuery Query { get; set; } = new();

        public AdminDashboardDto DashboardData { get; set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            DashboardData = await _api.GetAsync<AdminDashboardDto>(
                $"/api/admin/dashboard{ApiQueryStringBuilder.Build(Query)}", ct) ?? new();
        }
    }
}
