using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class PromotionCampaign : Entity<Guid>
    {
        public Guid StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int TotalQuantityLimit { get; set; }
        public int ReservedQuantity { get; set; }
        public int RedeemedQuantity { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public Store Store { get; set; } = null!;
        public ICollection<PromotionProduct> Products { get; set; } = new List<PromotionProduct>();
        public ICollection<PromotionReservation> Reservations { get; set; } = new List<PromotionReservation>();

        public int RemainingQuantity =>
            Math.Max(0, TotalQuantityLimit - ReservedQuantity - RedeemedQuantity);

        public PromotionStatus GetStatus(DateTime utcNow)
        {
            if (utcNow >= EndAt)
            {
                return PromotionStatus.Expired;
            }

            if (!IsEnabled)
            {
                return PromotionStatus.Paused;
            }

            if (utcNow < StartAt)
            {
                return PromotionStatus.Upcoming;
            }

            return RemainingQuantity == 0
                ? PromotionStatus.Exhausted
                : PromotionStatus.Active;
        }
    }
}
