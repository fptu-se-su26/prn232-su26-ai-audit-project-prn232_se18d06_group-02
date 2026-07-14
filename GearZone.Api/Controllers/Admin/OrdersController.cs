using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/orders")]
[ApiController]
public class OrdersController : BaseApiController
{
    private readonly IAdminOrderService _orderService;

    public OrdersController(IAdminOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET /api/admin/orders?[query]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AdminOrderQueryDto query)
    {
        var orders = await _orderService.GetOrdersAsync(query);
        var stats = await _orderService.GetOrderStatsAsync();
        return OkResponse(new { orders, stats });
    }

    // GET /api/admin/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var order = await _orderService.GetOrderDetailAsync(id);
        if (order == null) return FailResponse("Order not found.", 404);
        return OkResponse(order);
    }
}
