using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Web.Hubs
{
    public class SignalROrderTrackingNotifier : IOrderTrackingNotifier
    {
        private readonly IHubContext<OrderTrackingHub> _hubContext;
        private readonly ISubOrderRepository _subOrderRepository;

        public SignalROrderTrackingNotifier(
            IHubContext<OrderTrackingHub> hubContext,
            ISubOrderRepository subOrderRepository)
        {
            _hubContext = hubContext;
            _subOrderRepository = subOrderRepository;
        }

        public async Task NotifySubOrderUpdatedAsync(Guid subOrderId, CancellationToken ct = default)
        {
            if (subOrderId == Guid.Empty)
            {
                return;
            }

            var payload = new
            {
                subOrderId,
                updatedAtIso = DateTime.UtcNow.ToString("O")
            };

            await _hubContext.Clients.Group(OrderTrackingHub.GetGroupName(subOrderId))
                .SendAsync("TrackingUpdated", payload, ct);

            var subOrder = await _subOrderRepository.Query()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == subOrderId, ct);

            if (subOrder != null && subOrder.Order != null && !string.IsNullOrWhiteSpace(subOrder.Order.UserId))
            {
                var buyerPayload = new
                {
                    subOrderId = subOrder.Id,
                    orderCode = subOrder.Order.OrderCode,
                    status = subOrder.Status.ToString()
                };

                await _hubContext.Clients
                    .Group(OrderTrackingHub.GetBuyerOrdersGroupName(subOrder.Order.UserId))
                    .SendAsync("BuyerOrderStatusChanged", buyerPayload, ct);
            }
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
