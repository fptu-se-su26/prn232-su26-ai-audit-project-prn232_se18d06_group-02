using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class PaymentReturnModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public PaymentReturnModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(
            [FromQuery] long? orderCode,
            [FromQuery] string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            if (orderCode == null)
            {
                ErrorMessage = "Invalid payment return parameters.";
                return Page();
            }

            // PayOS sends status in the return URL query params
            // But we always verify with the API for security
            var result = await _paymentService.VerifyAndConfirmPaymentAsync(orderCode.Value);

            if (result.Success && result.OrderId.HasValue)
            {
                return RedirectToPage("./Success", new { orderId = result.OrderId.Value });
            }

            ErrorMessage = result.ErrorMessage ?? "Payment verification failed.";
            return Page();
        }
    }
}
