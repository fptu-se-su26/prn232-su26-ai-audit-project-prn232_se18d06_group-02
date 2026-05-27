using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Web.Controllers.Api.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/orders")]
[ApiController]
public class OrdersController : BaseApiController
{
    private readonly IChatService _chatService;
    private readonly IStoreRepository _storeRepository;
    private readonly ISubOrderRepository _subOrderRepository;

    public OrdersController(
        IChatService chatService,
        IStoreRepository storeRepository,
        ISubOrderRepository subOrderRepository)
    {
        _chatService = chatService;
        _storeRepository = storeRepository;
        _subOrderRepository = subOrderRepository;
    }

    // GET /api/seller/orders?search=&status=&page=&sort=&dir=&minSubtotal=&maxSubtotal=&dateRange=
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] SellerChatOrderQueryDto query)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize <= 0) query.PageSize = 10;

        var orders = await _chatService.GetSellerChatOrdersAsync(CurrentUserId!, query);

        var store = await _storeRepository.GetStoreByOwnerIdAsync(CurrentUserId!);
        object? stats = null;
        if (store != null)
        {
            var q = _subOrderRepository.Query().Where(x => x.StoreId == store.Id);
            stats = new
            {
                total = await q.CountAsync(),
                paid = await q.CountAsync(x => x.Status == OrderStatus.Paid || x.Status == OrderStatus.Processing || x.Status == OrderStatus.Delivered || x.Status == OrderStatus.Completed),
                unpaid = await q.CountAsync(x => x.Status == OrderStatus.Pending || x.Status == OrderStatus.AwaitingPayment || x.Status == OrderStatus.Approved),
                revenue = await q.Where(x => x.Status != OrderStatus.Cancelled && x.Status != OrderStatus.Rejected && x.Status != OrderStatus.Refunded).SumAsync(x => (decimal?)x.Subtotal) ?? 0m
            };
        }

        return OkResponse(new { orders, stats });
    }

    // GET /api/seller/orders/{subOrderId}
    [HttpGet("{subOrderId:guid}")]
    public async Task<IActionResult> Get(Guid subOrderId)
    {
        var detail = await _chatService.GetSellerChatOrderDetailAsync(CurrentUserId!, subOrderId);
        if (detail == null) return FailResponse("Order not found or access denied.", 404);
        return OkResponse(detail);
    }

    // POST /api/seller/orders/{subOrderId}/approve
    [HttpPost("{subOrderId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid subOrderId)
    {
        var ok = await _chatService.ApproveSellerOrderAsync(CurrentUserId!, subOrderId);
        if (ok) await _chatService.MarkSellerOrderProcessingAsync(CurrentUserId!, subOrderId);
        return ok
            ? OkResponse("Order approved and moved to processing.")
            : FailResponse("Cannot approve. Only pending orders can be approved.");
    }

    // POST /api/seller/orders/{subOrderId}/reject
    [HttpPost("{subOrderId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid subOrderId)
    {
        var ok = await _chatService.RejectSellerOrderAsync(CurrentUserId!, subOrderId);
        return ok
            ? OkResponse("Order rejected.")
            : FailResponse("Cannot reject. Only pending orders can be rejected.");
    }

    // POST /api/seller/orders/{subOrderId}/mark-processing
    [HttpPost("{subOrderId:guid}/mark-processing")]
    public async Task<IActionResult> MarkProcessing(Guid subOrderId)
    {
        var ok = await _chatService.MarkSellerOrderProcessingAsync(CurrentUserId!, subOrderId);
        return ok
            ? OkResponse("Order marked as processing.")
            : FailResponse("Cannot mark as processing. Valid states: Approved or Paid.");
    }

    // POST /api/seller/orders/{subOrderId}/mark-delivered
    [HttpPost("{subOrderId:guid}/mark-delivered")]
    public async Task<IActionResult> MarkDelivered(Guid subOrderId)
    {
        var ok = await _chatService.MarkSellerOrderDeliveredAsync(CurrentUserId!, subOrderId);
        return ok
            ? OkResponse("Order marked as delivered.")
            : FailResponse("Cannot mark as delivered. Valid state: Processing.");
    }
}
