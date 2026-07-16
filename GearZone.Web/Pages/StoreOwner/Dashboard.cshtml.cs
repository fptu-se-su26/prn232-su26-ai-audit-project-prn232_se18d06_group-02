using System.Security.Claims;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner
{
    [Authorize(Roles = "Store Owner")]
    public class DashboardModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of querying repositories in-process.
        private readonly IApiClient _api;

        public DashboardModel(IApiClient api)
        {
            _api = api;
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

        public List<SellerDashboardMonthlyPointDto> RevenueByMonth { get; set; } = new();
        public List<SellerDashboardRecentOrderDto> RecentOrders { get; set; } = new();
        public List<SellerDashboardRecentPayoutDto> RecentPayouts { get; set; } = new();

        public async Task OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var data = await _api.GetAsync<SellerDashboardDto>("/api/seller/dashboard", ct);
            if (data == null)
            {
                return;
            }

            CustomerConversationCount = data.CustomerConversationCount;
            CustomerUnreadCount = data.CustomerUnreadCount;

            if (!data.HasStore)
            {
                return;
            }

            HasStore = true;
            StoreName = data.StoreName;

            TotalOrders = data.TotalOrders;
            PendingOrders = data.PendingOrders;
            FulfilledOrders = data.FulfilledOrders;

            GrossRevenue = data.GrossRevenue;
            PaidOutAmount = data.PaidOutAmount;
            PendingPayoutAmount = data.PendingPayoutAmount;

            RevenueByMonth = data.RevenueByMonth;
            RecentOrders = data.RecentOrders;
            RecentPayouts = data.RecentPayouts;
        }
    }
}
