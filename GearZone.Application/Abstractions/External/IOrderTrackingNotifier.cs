namespace GearZone.Application.Abstractions.External
{
    public interface IOrderTrackingNotifier
    {
        Task NotifySubOrderUpdatedAsync(Guid subOrderId, CancellationToken ct = default);
    }
}