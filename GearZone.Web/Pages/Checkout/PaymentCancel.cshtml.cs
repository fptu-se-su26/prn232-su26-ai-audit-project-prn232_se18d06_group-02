using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class PaymentCancelModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<PaymentCancelModel> _logger;

        public PaymentCancelModel(
            IOrderService orderService,
            ILogger<PaymentCancelModel> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public Guid? OrderId { get; set; }
        public long? OrderCode { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] Guid? orderId, [FromQuery] long? orderCode, [FromQuery] bool processed = false)
        {
            OrderId = orderId;
            OrderCode = orderCode;
            _logger.LogInformation("PaymentCancel page loaded with OrderId: {OrderId}, OrderCode: {OrderCode}", orderId, orderCode);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (processed)
            {
                return Page();
            }

            if (orderId.HasValue)
            {
                await _orderService.CancelOrderAsync(orderId.Value, userIdStr);
            }
            else if (orderCode.HasValue)
            {
                var order = await _orderService.GetOrderByOrderCodeAsync(orderCode.Value);
                if (order != null)
                {
                    await _orderService.CancelOrderAsync(order.Id, userIdStr);
                }
                else
                {
                    _logger.LogWarning("PaymentCancel: Could not find order with OrderCode {OrderCode}", orderCode.Value);
                }
            }

            return Page();
        }
    }
}
