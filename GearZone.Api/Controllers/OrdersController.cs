using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IAuthService _authService;

    public OrdersController(IOrderService orderService, IPaymentService paymentService, IAuthService authService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _authService = authService;
    }

    // GET /api/orders/{orderId}/payment-status
    [HttpGet("{orderId:guid}/payment-status")]
    public async Task<IActionResult> GetPaymentStatus(Guid orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null) return FailResponse("Order not found.", 404);

        string overallStatus;

        if (order.SubOrders.Any(so => so.Status == OrderStatus.AwaitingPayment))
        {
            var verifyResult = await _paymentService.VerifyAndConfirmPaymentAsync(order.OrderCode);
            overallStatus = verifyResult.Success ? "Paid" : "AwaitingPayment";
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
            overallStatus = order.SubOrders.FirstOrDefault()?.Status.ToString() ?? "Unknown";
        }

        return OkResponse(new { status = overallStatus });
    }

    // GET /api/orders/track/{subOrderId}
    [HttpGet("track/{subOrderId:guid}")]
    public async Task<IActionResult> Track(Guid subOrderId)
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        if (subOrderId == Guid.Empty) return FailResponse("Invalid order ID.");

        var tracking = await _orderService.GetUserOrderTrackingAsync(user.Id, subOrderId);
        if (tracking == null) return FailResponse("Order not found.", 404);

        return OkResponse(tracking);
    }

    // GET /api/orders/track/{subOrderId}/live
    [HttpGet("track/{subOrderId:guid}/live")]
    public async Task<IActionResult> TrackLive(Guid subOrderId, CancellationToken ct)
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        if (subOrderId == Guid.Empty) return FailResponse("Invalid order ID.");

        var tracking = await _orderService.GetUserOrderTrackingAsync(user.Id, subOrderId, ct);
        if (tracking == null) return FailResponse("Order not found.", 404);

        return OkResponse(UserOrderTrackingLiveDto.From(tracking));
    }
}
