using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Orders.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace GearZone.Web.Pages.Public.User.Orders
{
    [Authorize]
    public class TrackModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public TrackModel(IOrderService orderService, IAuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        public UserOrderTrackingDto Tracking { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid subOrderId)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            if (subOrderId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders" });
            }

            var tracking = await _orderService.GetUserOrderTrackingAsync(user.Id, subOrderId);
            if (tracking == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders" });
            }

            Tracking = tracking;
            return Page();
        }

        public async Task<IActionResult> OnGetLiveAsync(Guid subOrderId, CancellationToken ct = default)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (subOrderId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid order." });
            }

            var tracking = await _orderService.GetUserOrderTrackingAsync(user.Id, subOrderId, ct);
            if (tracking == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            var history = tracking.StatusHistory
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new
                {
                    changedAtIso = x.ChangedAt.ToString("O"),
                    oldStatus = x.OldStatus?.ToString(),
                    newStatus = x.NewStatus.ToString(),
                    changedByDisplayName = x.ChangedByDisplayName,
                    note = x.Note
                })
                .ToList();

            return new JsonResult(new
            {
                subOrderId = tracking.SubOrderId,
                status = tracking.Status.ToString(),
                shippingProvider = tracking.ShippingProvider,
                trackingNumber = tracking.TrackingNumber,
                deliveredAtIso = tracking.DeliveredAt?.ToString("O"),
                statusHistory = history
            });
        }
    }
}
