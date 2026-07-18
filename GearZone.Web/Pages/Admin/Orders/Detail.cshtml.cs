using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;

namespace GearZone.Web.Pages.Admin.Orders
{
    [Authorize(Roles = "Super Admin")]
    public class DetailModel : PageModel
    {
        private readonly IApiClient _api;

        public DetailModel(IApiClient api)
        {
            _api = api;
        }

        public AdminOrderDetailDto Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid? id, CancellationToken ct)
        {
            if (id == null) return NotFound();

            var order = await _api.GetAsync<AdminOrderDetailDto>($"/api/admin/orders/{id.Value}", ct);
            if (order == null) return NotFound();

            Order = order;
            return Page();
        }
    }
}
