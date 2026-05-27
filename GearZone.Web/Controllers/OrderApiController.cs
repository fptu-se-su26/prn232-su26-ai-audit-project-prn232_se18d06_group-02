using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrderApiController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpGet("{orderId:guid}/payment-status")]
        public async Task<IActionResult> GetPaymentStatus(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return NotFound(new { error = "Order not found" });
            }

            // Determine overall status based on sub-orders
            string overallStatus = "Unknown";

            if (order.SubOrders.Any(so => so.Status == OrderStatus.AwaitingPayment))
            {
                // Actively check payment gateway status since webhooks might be blocked locally
                // and iframes cannot redirect.
                var verifyResult = await _paymentService.VerifyAndConfirmPaymentAsync(order.OrderCode);
                
                if (verifyResult.Success)
                {
                    overallStatus = "Paid";
                }
                else
                {
                    overallStatus = "AwaitingPayment";
                }
            }
            else if (order.SubOrders.All(so => so.Status == OrderStatus.Paid))
            {
                overallStatus = "Paid";
            }
            else if (order.SubOrders.Any(so => so.Status == OrderStatus.Cancelled))
            {
                overallStatus = "Cancelled";
            }
            else
            {
                // Fallback to the first suborder's status if mixed or different
                overallStatus = order.SubOrders.FirstOrDefault()?.Status.ToString() ?? "Unknown";
            }

            return Ok(new { status = overallStatus });
        }
    }
}
