using GearZone.Api.Controllers;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var userId = CurrentUserId!;

        var inbox = await _chatService.GetSellerInboxAsync(userId, new ChatInboxQueryDto { PageNumber = 1, PageSize = 1 });
        var dto = new SellerDashboardDto
        {
            CustomerConversationCount = inbox.TotalCount,
            CustomerUnreadCount = await _chatService.GetSellerUnreadCountAsync(userId)
        };

        var store = await _storeRepository.GetStoreByOwnerIdAsync(userId);
        if (store == null) return OkResponse(dto); // HasStore stays false

        dto.HasStore = true;
        dto.StoreName = store.StoreName;

        var orderQuery = _subOrderRepository.Query().Where(x => x.StoreId == store.Id);

        dto.TotalOrders = await orderQuery.CountAsync(ct);
        dto.PendingOrders = await orderQuery.CountAsync(x =>
            x.Status == OrderStatus.Pending || x.Status == OrderStatus.AwaitingPayment || x.Status == OrderStatus.Approved, ct);
        dto.FulfilledOrders = await orderQuery.CountAsync(x =>
            x.Status == OrderStatus.Paid || x.Status == OrderStatus.Processing ||
            x.Status == OrderStatus.Delivered || x.Status == OrderStatus.Completed, ct);

        dto.GrossRevenue = await orderQuery
            .Where(x => x.Status != OrderStatus.Cancelled && x.Status != OrderStatus.Rejected && x.Status != OrderStatus.Refunded)
            .SumAsync(x => (decimal?)x.Subtotal, ct) ?? 0m;

        var now = DateTime.UtcNow;
        var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var monthlyData = await orderQuery
            .Where(x => x.CreatedAt >= startMonth)
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Revenue = g.Sum(x => x.Subtotal) })
            .ToListAsync(ct);

        for (var i = 0; i < 6; i++)
        {
            var month = startMonth.AddMonths(i);
            var value = monthlyData.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);
            dto.RevenueByMonth.Add(new SellerDashboardMonthlyPointDto
            {
                Label = month.ToString("MMM"),
                Revenue = value?.Revenue ?? 0m
            });
        }

        dto.RecentOrders = await orderQuery
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new SellerDashboardRecentOrderDto
            {
                SubOrderId = x.Id,
                OrderCode = x.Order.OrderCode,
                BuyerName = x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email ?? "Buyer",
                Status = x.Status,
                Subtotal = x.Subtotal,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        var payoutQuery = _payoutTransactionRepository.Query().Where(x => x.StoreId == store.Id);

        dto.PaidOutAmount = await payoutQuery
            .Where(x => x.Status == PayoutTransactionStatus.Success)
            .SumAsync(x => (decimal?)x.NetAmount, ct) ?? 0m;

        dto.PendingPayoutAmount = await payoutQuery
            .Where(x => x.Status == PayoutTransactionStatus.Queued ||
                        x.Status == PayoutTransactionStatus.Processing ||
                        x.Status == PayoutTransactionStatus.ManualRequired)
            .SumAsync(x => (decimal?)x.NetAmount, ct) ?? 0m;

        dto.RecentPayouts = await payoutQuery
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new SellerDashboardRecentPayoutDto
            {
                TransactionCode = x.TransactionCode,
                NetAmount = x.NetAmount,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return OkResponse(dto);
    }
}
