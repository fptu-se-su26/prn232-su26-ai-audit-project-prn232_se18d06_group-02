using GearZone.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GearZone.Web.Hubs
{
    [Authorize]
    public class OrderTrackingHub : Hub
    {
        private readonly IOrderService _orderService;

        public OrderTrackingHub(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task JoinTracking(Guid subOrderId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Authentication is required.");
            }

            if (subOrderId == Guid.Empty)
            {
                throw new HubException("Invalid order.");
            }

            var tracking = await _orderService.GetUserOrderTrackingAsync(userId, subOrderId);
            if (tracking == null)
            {
                throw new HubException("Order not found.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(subOrderId));
        }

        public Task LeaveTracking(Guid subOrderId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(subOrderId));
        }

        public async Task JoinSellerOrders()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) ||
                Context.User?.IsInRole("Store Owner") != true)
            {
                throw new HubException("Seller access is required.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetSellerOrdersGroupName(userId));
        }

        public Task LeaveSellerOrders()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId)
                ? Task.CompletedTask
                : Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    GetSellerOrdersGroupName(userId));
        }

        public static string GetGroupName(Guid subOrderId)
        {
            return $"order-tracking:{subOrderId}";
        }

        public static string GetSellerOrdersGroupName(string sellerUserId)
        {
            return $"seller-orders:{sellerUserId}";
        }

        public async Task JoinBuyerOrders()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Authentication required.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetBuyerOrdersGroupName(userId));
        }

        public Task LeaveBuyerOrders()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId)
                ? Task.CompletedTask
                : Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    GetBuyerOrdersGroupName(userId));
        }

        public static string GetBuyerOrdersGroupName(string buyerUserId)
        {
            return $"buyer-orders:{buyerUserId}";
        }
    }
}
