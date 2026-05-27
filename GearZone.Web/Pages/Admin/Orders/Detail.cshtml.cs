using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Web.Pages.Admin.Orders
{
    [Authorize(Roles = "Super Admin")]
    public class DetailModel : PageModel
    {
        private readonly IAdminOrderService _orderService;

        public DetailModel(IAdminOrderService orderService)
        {
            _orderService = orderService;
        }

        public AdminOrderDetailDto Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null) return NotFound();

            var order = await _orderService.GetOrderDetailAsync(id.Value);
            if (order == null) return NotFound();

            Order = order;
            return Page();
        }
    }
}
