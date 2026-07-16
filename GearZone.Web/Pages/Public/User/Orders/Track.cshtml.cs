using GearZone.Application.Features.Orders.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.User.Orders
{
    [Authorize]
    public class TrackModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the order service in-process.
        private readonly IApiClient _api;

        public TrackModel(IApiClient api)
        {
            _api = api;
        }

        public UserOrderTrackingDto Tracking { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid subOrderId, CancellationToken ct)
        {
            if (subOrderId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders" });
            }

            var tracking = await _api.GetAsync<UserOrderTrackingDto>($"/api/orders/track/{subOrderId}", ct);
            if (tracking == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders" });
            }

            Tracking = tracking;
            return Page();
        }

        // Polled by the page's script every few seconds and after each SignalR ping, so it
        // returns the API payload as-is rather than the ApiResponse envelope.
        public async Task<IActionResult> OnGetLiveAsync(Guid subOrderId, CancellationToken ct = default)
        {
            if (subOrderId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid order." });
            }

            var live = await _api.GetAsync<UserOrderTrackingLiveDto>($"/api/orders/track/{subOrderId}/live", ct);
            if (live == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            return new JsonResult(live);
        }
    }
}
