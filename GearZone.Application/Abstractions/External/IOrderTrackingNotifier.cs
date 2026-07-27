namespace GearZone.Application.Abstractions.External
{
    public interface IOrderTrackingNotifier
    {
        Task NotifySubOrderUpdatedAsync(Guid subOrderId, CancellationToken ct = default);
        Task NotifySellerOrderCreatedAsync(
            SellerOrderCreatedNotification notification,
            CancellationToken ct = default);
    }

    public class SellerOrderCreatedNotification
    {
        public string SellerUserId { get; set; } = string.Empty;
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public string BuyerDisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
        public string ProductPreview { get; set; } = string.Empty;
    }
}
