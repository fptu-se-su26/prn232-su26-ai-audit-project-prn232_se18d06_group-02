using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Orders
{
    [Authorize(Roles = "Store Owner")]
    public class DetailModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public SellerChatOrderDetailDto OrderDetail { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            if (subOrderId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid order.";
                return RedirectToPage("/StoreOwner/Orders/Index");
            }

            var detail = await LoadOrderDetailAsync(userId, subOrderId);
            if (detail == null)
            {
                return RedirectToPage("/StoreOwner/Orders/Index");
            }

            OrderDetail = detail;

            ViewData["Title"] = $"Order #{detail.OrderCode}";
            ViewData["PageHeader"] = "Order Detail";
            ViewData["ActivePage"] = "Orders";
            ViewData["Breadcrumb"] = new[] { "Orders", $"#{detail.OrderCode}" };

            return Page();
        }

        public async Task<IActionResult> OnPostMarkProcessingAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.MarkSellerOrderProcessingAsync(userId, subOrderId);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order marked as processing."
                : "Cannot mark this order as processing. Valid states: Approved or Paid.";

            return RedirectToPage(new { subOrderId });
        }

        public async Task<IActionResult> OnPostMarkDeliveredAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.MarkSellerOrderDeliveredAsync(userId, subOrderId);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order marked as delivered."
                : "Cannot mark this order as delivered. Valid state: Processing.";

            return RedirectToPage(new { subOrderId });
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.ApproveSellerOrderAsync(userId, subOrderId);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order approved successfully."
                : "Cannot approve this order. Only pending orders can be approved.";

            return RedirectToPage(new { subOrderId });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.RejectSellerOrderAsync(userId, subOrderId);
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order rejected successfully."
                : "Cannot reject this order. Only pending orders can be rejected.";

            return RedirectToPage(new { subOrderId });
        }

        private async Task<SellerChatOrderDetailDto?> LoadOrderDetailAsync(string userId, Guid subOrderId)
        {
            var detail = await _chatService.GetSellerChatOrderDetailAsync(userId, subOrderId);
            if (detail == null)
            {
                TempData["ErrorMessage"] = "Order not found or you do not have permission.";
                return null;
            }

            return detail;
        }
    }
}
