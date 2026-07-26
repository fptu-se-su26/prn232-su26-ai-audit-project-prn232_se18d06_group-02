using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class PromotionReservation : Entity<Guid>
    {
        public Guid CampaignId { get; set; }
        public Guid OrderId { get; set; }
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
        public PromotionReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }

        public PromotionCampaign Campaign { get; set; } = null!;
        public Order Order { get; set; } = null!;
        public OrderItem OrderItem { get; set; } = null!;
    }
}
