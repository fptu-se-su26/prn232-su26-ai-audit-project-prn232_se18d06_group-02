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
    }
}