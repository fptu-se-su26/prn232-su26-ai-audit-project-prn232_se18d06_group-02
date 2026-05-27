using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearZone.Web.Pages.StoreOwner
{
    [Authorize(Roles = "Store Owner")]
    public class DashboardModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IStoreRepository _storeRepository;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IPayoutTransactionRepository _payoutTransactionRepository;

        public DashboardModel(
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

        public bool HasStore { get; set; }
        public string StoreName { get; set; } = "Your Store";
        public int CustomerConversationCount { get; set; }
        public int CustomerUnreadCount { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int FulfilledOrders { get; set; }

        public decimal GrossRevenue { get; set; }
        public decimal PaidOutAmount { get; set; }
        public decimal PendingPayoutAmount { get; set; }

        public List<MonthlyRevenuePoint> RevenueByMonth { get; set; } = new();
        public List<RecentOrderItem> RecentOrders { get; set; } = new();
        public List<RecentPayoutItem> RecentPayouts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var inbox = await _chatService.GetSellerInboxAsync(userId, new ChatInboxQueryDto
            {
                PageNumber = 1,
                PageSize = 1
            });

            CustomerConversationCount = inbox.TotalCount;
            CustomerUnreadCount = await _chatService.GetSellerUnreadCountAsync(userId);

            var store = await _storeRepository.GetStoreByOwnerIdAsync(userId);
            if (store == null)
            {
                return;
            }

            HasStore = true;
            StoreName = store.StoreName;

            var orderQuery = _subOrderRepository.Query().Where(x => x.StoreId == store.Id);

            TotalOrders = await orderQuery.CountAsync();
            PendingOrders = await orderQuery.CountAsync(x =>
                x.Status == OrderStatus.Pending ||
                x.Status == OrderStatus.AwaitingPayment ||
                x.Status == OrderStatus.Approved);
            FulfilledOrders = await orderQuery.CountAsync(x =>
                x.Status == OrderStatus.Paid ||
                x.Status == OrderStatus.Processing ||
                x.Status == OrderStatus.Delivered ||
                x.Status == OrderStatus.Completed);

            GrossRevenue = await orderQuery
                .Where(x =>
                    x.Status != OrderStatus.Cancelled &&
                    x.Status != OrderStatus.Rejected &&
                    x.Status != OrderStatus.Refunded)
                .SumAsync(x => (decimal?)x.Subtotal) ?? 0m;

            var now = DateTime.UtcNow;
            var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var monthlyData = await orderQuery
                .Where(x => x.CreatedAt >= startMonth)
                .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => x.Subtotal)
                })
                .ToListAsync();

            for (var i = 0; i < 6; i++)
            {
                var month = startMonth.AddMonths(i);
                var monthValue = monthlyData.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month);
                RevenueByMonth.Add(new MonthlyRevenuePoint
                {
                    Label = month.ToString("MMM"),
                    Revenue = monthValue?.Revenue ?? 0m
                });
            }

            RecentOrders = await orderQuery
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new RecentOrderItem
                {
                    SubOrderId = x.Id,
                    OrderCode = x.Order.OrderCode,
                    BuyerName = x.Order.User.FullName ?? x.Order.User.UserName ?? x.Order.User.Email ?? "Buyer",
                    Status = x.Status,
                    Subtotal = x.Subtotal,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var payoutQuery = _payoutTransactionRepository.Query().Where(x => x.StoreId == store.Id);

            PaidOutAmount = await payoutQuery
                .Where(x => x.Status == PayoutTransactionStatus.Success)
                .SumAsync(x => (decimal?)x.NetAmount) ?? 0m;

            PendingPayoutAmount = await payoutQuery
                .Where(x =>
                    x.Status == PayoutTransactionStatus.Queued ||
                    x.Status == PayoutTransactionStatus.Processing ||
                    x.Status == PayoutTransactionStatus.ManualRequired)
                .SumAsync(x => (decimal?)x.NetAmount) ?? 0m;

            RecentPayouts = await payoutQuery
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new RecentPayoutItem
                {
                    TransactionCode = x.TransactionCode,
                    NetAmount = x.NetAmount,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public class MonthlyRevenuePoint
        {
            public string Label { get; set; } = string.Empty;
            public decimal Revenue { get; set; }
        }

        public class RecentOrderItem
        {
            public Guid SubOrderId { get; set; }
            public long OrderCode { get; set; }
            public string BuyerName { get; set; } = string.Empty;
            public OrderStatus Status { get; set; }
            public decimal Subtotal { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class RecentPayoutItem
        {
            public string TransactionCode { get; set; } = string.Empty;
            public decimal NetAmount { get; set; }
            public PayoutTransactionStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
