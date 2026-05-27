using GearZone.Application.Abstractions.External;

namespace GearZone.Application.Features.Orders
{
    public class NoOpOrderTrackingNotifier : IOrderTrackingNotifier
    {
        public Task NotifySubOrderUpdatedAsync(Guid subOrderId, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}