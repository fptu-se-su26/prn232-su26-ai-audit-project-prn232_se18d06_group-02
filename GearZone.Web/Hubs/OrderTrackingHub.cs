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

        public static string GetGroupName(Guid subOrderId)
        {
            return $"order-tracking:{subOrderId}";
        }
    }
}