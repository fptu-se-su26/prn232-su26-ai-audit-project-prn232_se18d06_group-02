using GearZone.Application.Abstractions.External;
using Microsoft.AspNetCore.SignalR;

namespace GearZone.Web.Hubs
{
    public class SignalROrderTrackingNotifier : IOrderTrackingNotifier
    {
        private readonly IHubContext<OrderTrackingHub> _hubContext;

        public SignalROrderTrackingNotifier(IHubContext<OrderTrackingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifySubOrderUpdatedAsync(Guid subOrderId, CancellationToken ct = default)
        {
            if (subOrderId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            var payload = new
            {
                subOrderId,
                updatedAtIso = DateTime.UtcNow.ToString("O")
            };

            return _hubContext.Clients.Group(OrderTrackingHub.GetGroupName(subOrderId))
                .SendAsync("TrackingUpdated", payload, ct);
        }

        public Task NotifySellerOrderCreatedAsync(
            SellerOrderCreatedNotification notification,
            CancellationToken ct = default)
        {
            if (notification == null ||
                string.IsNullOrWhiteSpace(notification.SellerUserId) ||
                notification.SubOrderId == Guid.Empty)
            {
                return Task.CompletedTask;
            }

            var payload = new
            {
                subOrderId = notification.SubOrderId,
                orderCode = notification.OrderCode,
                buyerDisplayName = notification.BuyerDisplayName,
                createdAt = notification.CreatedAt,
                status = notification.Status,
                subtotal = notification.Subtotal,
                itemCount = notification.ItemCount,
                productPreview = notification.ProductPreview,
                notifiedAtIso = DateTime.UtcNow.ToString("O")
            };

            return _hubContext.Clients
                .Group(OrderTrackingHub.GetSellerOrdersGroupName(notification.SellerUserId))
                .SendAsync("SellerOrderCreated", payload, ct);
        }
    }
}
