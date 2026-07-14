using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Enums;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearZone.Api.Controllers.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/dashboard")]
[ApiController]
public class DashboardController : BaseApiController
{
    private readonly IChatService _chatService;
    private readonly IStoreRepository _storeRepository;
    private readonly ISubOrderRepository _subOrderRepository;
    private readonly IPayoutTransactionRepository _payoutTransactionRepository;

    public DashboardController(
        IChatService chatService,
        IStoreRepository storeRepository,
        ISubOrderRepository subOrderRepository,
        IPayoutTransactionRepository payoutTransactionRepository)
    {
        _chatService = chatService;
        _storeRepository = storeRepository;
        _subOrderRepository = subOrderRepository;
        _payoutTransactionRepository = payoutTransactionRepository;
    }

    // GET /api/seller/dashboard
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var inbox = await _chatService.GetSellerInboxAsync(userId, new ChatInboxQueryDto { PageNumber = 1, PageSize = 1 });
        var unreadCount = await _chatService.GetSellerUnreadCountAsync(userId);

        var store = await _storeRepository.GetStoreByOwnerIdAsync(userId);
        if (store == null) return OkResponse(new { hasStore = false, conversationCount = inbox.TotalCount, unreadCount });

        var orderQuery = _subOrderRepository.Query().Where(x => x.StoreId == store.Id);

        var totalOrders = await orderQuery.CountAsync();
        var pendingOrders = await orderQuery.CountAsync(x =>
            x.Status == OrderStatus.Pending || x.Status == OrderStatus.AwaitingPayment || x.Status == OrderStatus.Approved);
        var fulfilledOrders = await orderQuery.CountAsync(x =>
            x.Status == OrderStatus.Paid || x.Status == OrderStatus.Processing ||
            x.Status == OrderStatus.Delivered || x.Status == OrderStatus.Completed);

        var grossRevenue = await orderQuery
            .Where(x => x.Status != OrderStatus.Cancelled && x.Status != OrderStatus.Rejected && x.Status != OrderStatus.Refunded)
            .SumAsync(x => (decimal?)x.Subtotal) ?? 0m;

        var now = DateTime.UtcNow;
        var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var monthlyRaw = await orderQuery
            .Where(x => x.CreatedAt >= startMonth)
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Subtotal) })
            .ToListAsync();

        var revenueByMonth = Enumerable.Range(0, 6).Select(i =>
        {
            var m = startMonth.AddMonths(i);
            var val = monthlyRaw.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
            return new { label = m.ToString("MMM"), revenue = val?.Revenue ?? 0m };
        }).ToList();

        var recentOrders = await orderQuery
            .OrderByDescending(x => x.CreatedAt).Take(5)
            .Select(x => new
            {
                subOrderId = x.Id,
                orderCode = x.Order.OrderCode,
                buyerName = x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email ?? "Buyer",
                status = x.Status.ToString(),
                subtotal = x.Subtotal,
                createdAt = x.CreatedAt
            }).ToListAsync();

        var payoutQuery = _payoutTransactionRepository.Query().Where(x => x.StoreId == store.Id);
        var paidOut = await payoutQuery.Where(x => x.Status == PayoutTransactionStatus.Success).SumAsync(x => (decimal?)x.NetAmount) ?? 0m;
        var pendingPayout = await payoutQuery
            .Where(x => x.Status == PayoutTransactionStatus.Queued || x.Status == PayoutTransactionStatus.Processing || x.Status == PayoutTransactionStatus.ManualRequired)
            .SumAsync(x => (decimal?)x.NetAmount) ?? 0m;

        var recentPayouts = await payoutQuery
            .OrderByDescending(x => x.CreatedAt).Take(5)
            .Select(x => new { x.TransactionCode, x.NetAmount, status = x.Status.ToString(), x.CreatedAt })
            .ToListAsync();

        return OkResponse(new
        {
            hasStore = true,
            storeName = store.StoreName,
            conversationCount = inbox.TotalCount,
            unreadCount,
            totalOrders, pendingOrders, fulfilledOrders,
            grossRevenue, paidOut, pendingPayout,
            revenueByMonth, recentOrders, recentPayouts
        });
    }
}
