using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Web.Pages.StoreOwner.Orders
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private static readonly OrderStatus[] NonRevenueStatuses =
        {
            OrderStatus.Cancelled,
            OrderStatus.Rejected,
            OrderStatus.Refunded
        };

        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStoreRepository _storeRepository;
        private readonly ISubOrderRepository _subOrderRepository;

        public IndexModel(
            IChatService chatService,
            UserManager<ApplicationUser> userManager,
            IStoreRepository storeRepository,
            ISubOrderRepository subOrderRepository)
        {
            _chatService = chatService;
            _userManager = userManager;
            _storeRepository = storeRepository;
            _subOrderRepository = subOrderRepository;
        }

        public PagedResult<SellerChatOrderListItemDto> Orders { get; set; } = new();
        public SellerOrderStatsViewModel Stats { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public OrderStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "createdAt";

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; } = "desc";

        [BindProperty(SupportsGet = true)]
        public decimal? MinSubtotal { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxSubtotal { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRangeShortcut { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateRange { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            PageNumber = PageNumber < 1 ? 1 : PageNumber;
            SortBy = string.IsNullOrWhiteSpace(SortBy) ? "createdAt" : SortBy;
            SortDirection = string.Equals(SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";

            var (startDate, endDate) = ResolveDateRange();

            Orders = await _chatService.GetSellerChatOrdersAsync(userId, new SellerChatOrderQueryDto
            {
                SearchTerm = SearchTerm,
                PageNumber = PageNumber,
                PageSize = 10,
                Status = Status,
                MinSubtotal = MinSubtotal,
                MaxSubtotal = MaxSubtotal,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = SortBy,
                SortDirection = SortDirection
            });

            await LoadStatsAsync(userId);
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid subOrderId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            var ok = await _chatService.ApproveSellerOrderAsync(userId, subOrderId);
            if (ok)
            {
                await _chatService.MarkSellerOrderProcessingAsync(userId, subOrderId);
            }

            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Order approved and moved to processing."
                : "Cannot approve this order. Only pending orders can be approved.";

            return RedirectToPage(new
            {
                SearchTerm,
                PageNumber,
                Status,
                SortBy,
                SortDirection,
                MinSubtotal,
                MaxSubtotal,
                DateRangeShortcut,
                DateRange
            });
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

            return RedirectToPage(new
            {
                SearchTerm,
                PageNumber,
                Status,
                SortBy,
                SortDirection,
                MinSubtotal,
                MaxSubtotal,
                DateRangeShortcut,
                DateRange
            });
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

            return RedirectToPage(new
            {
                SearchTerm,
                PageNumber,
                Status,
                SortBy,
                SortDirection,
                MinSubtotal,
                MaxSubtotal,
                DateRangeShortcut,
                DateRange
            });
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

            return RedirectToPage(new
            {
                SearchTerm,
                PageNumber,
                Status,
                SortBy,
                SortDirection,
                MinSubtotal,
                MaxSubtotal,
                DateRangeShortcut,
                DateRange
            });
        }

        private (DateTime? StartDate, DateTime? EndDate) ResolveDateRange()
        {
            if (string.IsNullOrWhiteSpace(DateRangeShortcut))
            {
                return (null, null);
            }

            var today = DateTime.UtcNow.Date;
            return DateRangeShortcut.ToLowerInvariant() switch
            {
                "today" => (today, today),
                "week" => (today.AddDays(-7), today),
                "month" => (today.AddDays(-30), today),
                "custom" => ParseCustomDateRange(),
                _ => (null, null)
            };
        }

        private (DateTime? StartDate, DateTime? EndDate) ParseCustomDateRange()
        {
            if (string.IsNullOrWhiteSpace(DateRange))
            {
                return (null, null);
            }

            var dates = DateRange.Split(" to ", StringSplitOptions.RemoveEmptyEntries);
            if (dates.Length == 2)
            {
                var start = DateTime.TryParse(dates[0], out var parsedStart) ? parsedStart : (DateTime?)null;
                var end = DateTime.TryParse(dates[1], out var parsedEnd) ? parsedEnd : (DateTime?)null;
                return (start, end);
            }

            if (dates.Length == 1 && DateTime.TryParse(dates[0], out var singleDate))
            {
                return (singleDate, singleDate);
            }

            return (null, null);
        }

        private async Task LoadStatsAsync(string ownerUserId)
        {
            var store = await _storeRepository.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                Stats = new SellerOrderStatsViewModel();
                return;
            }

            var orderQuery = _subOrderRepository.Query().Where(x => x.StoreId == store.Id);

            Stats.TotalOrders = await orderQuery.CountAsync();
            Stats.PaidOrders = await orderQuery.CountAsync(x =>
                x.Status == OrderStatus.Paid ||
                x.Status == OrderStatus.Processing ||
                x.Status == OrderStatus.Delivered ||
                x.Status == OrderStatus.Completed);
            Stats.UnpaidOrders = await orderQuery.CountAsync(x =>
                x.Status == OrderStatus.Pending ||
                x.Status == OrderStatus.AwaitingPayment ||
                x.Status == OrderStatus.Approved);
            Stats.TotalRevenue = await orderQuery
                .Where(x => !NonRevenueStatuses.Contains(x.Status))
                .SumAsync(x => (decimal?)x.Subtotal) ?? 0m;
        }

        public class SellerOrderStatsViewModel
        {
            public int TotalOrders { get; set; }
            public int PaidOrders { get; set; }
            public int UnpaidOrders { get; set; }
            public decimal TotalRevenue { get; set; }
        }
    }
}
