namespace GearZone.Application.Features.Orders.Dtos
{
    /// <summary>
    /// Flattened tracking snapshot the Track page polls for. Statuses and timestamps are
    /// pre-rendered as strings because the browser consumes this payload directly.
    /// </summary>
    public class UserOrderTrackingLiveDto
    {
        public Guid SubOrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ShippingProvider { get; set; }
        public string? TrackingNumber { get; set; }
        public string? DeliveredAtIso { get; set; }
        public List<UserOrderTrackingLiveHistoryDto> StatusHistory { get; set; } = new();

        public static UserOrderTrackingLiveDto From(UserOrderTrackingDto tracking) => new()
        {
            SubOrderId = tracking.SubOrderId,
            Status = tracking.Status.ToString(),
            ShippingProvider = tracking.ShippingProvider,
            TrackingNumber = tracking.TrackingNumber,
            DeliveredAtIso = tracking.DeliveredAt?.ToString("O"),
            StatusHistory = tracking.StatusHistory
                .OrderByDescending(x => x.ChangedAt)
                .Select(x => new UserOrderTrackingLiveHistoryDto
                {
                    ChangedAtIso = x.ChangedAt.ToString("O"),
                    OldStatus = x.OldStatus?.ToString(),
                    NewStatus = x.NewStatus.ToString(),
                    ChangedByDisplayName = x.ChangedByDisplayName,
                    Note = x.Note
                })
                .ToList()
        };
    }

    public class UserOrderTrackingLiveHistoryDto
    {
        public string ChangedAtIso { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string NewStatus { get; set; } = string.Empty;
        public string? ChangedByDisplayName { get; set; }
        public string? Note { get; set; }
    }
}
