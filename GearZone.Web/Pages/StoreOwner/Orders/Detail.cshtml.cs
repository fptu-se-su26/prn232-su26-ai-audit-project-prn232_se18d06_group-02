using System.Security.Claims;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Orders
{
    [Authorize(Roles = "Store Owner")]
    public class DetailModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the chat/order service in-process.
        private readonly IApiClient _api;

        public DetailModel(IApiClient api)
        {
            _api = api;
        }

        public SellerChatOrderDetailDto OrderDetail { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid subOrderId, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            if (subOrderId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage("/StoreOwner/Orders/Index");
            }

            var detail = await _api.GetAsync<SellerChatOrderDetailDto>($"/api/seller/orders/{subOrderId}", ct);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "Order not found or you do not have permission.";
                return RedirectToPage("/StoreOwner/Orders/Index");
            }

            OrderDetail = detail;

            ViewData["Title"] = $"Order #{detail.OrderCode}";
            ViewData["PageHeader"] = "Order Detail";
            ViewData["ActivePage"] = "Orders";
            ViewData["Breadcrumb"] = new[] { "Orders", $"#{detail.OrderCode}" };

            return Page();
        }

        public Task<IActionResult> OnPostMarkProcessingAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "mark-processing", "Order marked as processing.",
                "Cannot mark this order as processing. Valid states: Approved or Paid.", ct);

        public Task<IActionResult> OnPostMarkDeliveredAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "mark-delivered", "Order marked as delivered.",
                "Cannot mark this order as delivered. Valid state: Processing.", ct);

        public Task<IActionResult> OnPostApproveAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "approve", "Order approved successfully.",
                "Cannot approve this order. Only pending orders can be approved.", ct);

        public Task<IActionResult> OnPostRejectAsync(Guid subOrderId, CancellationToken ct) =>
            RunActionAsync(subOrderId, "reject", "Order rejected successfully.",
                "Cannot reject this order. Only pending orders can be rejected.", ct);

        private async Task<IActionResult> RunActionAsync(Guid subOrderId, string action, string successMessage, string errorMessage, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var result = await _api.PostAsync($"/api/seller/orders/{subOrderId}/{action}", ct);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Success ? successMessage : errorMessage;

            return RedirectToPage(new { subOrderId });
        }
    }
}
