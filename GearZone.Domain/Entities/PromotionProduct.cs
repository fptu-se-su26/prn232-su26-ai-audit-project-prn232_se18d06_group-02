namespace GearZone.Domain.Entities
{
    public class PromotionProduct
    {
        public Guid CampaignId { get; set; }
        public Guid ProductId { get; set; }

        public PromotionCampaign Campaign { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
