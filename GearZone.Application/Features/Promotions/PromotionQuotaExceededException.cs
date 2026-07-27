namespace GearZone.Application.Features.Promotions
{
    public class PromotionQuotaExceededException : InvalidOperationException
    {
        public PromotionQuotaExceededException()
            : base("A promotion campaign no longer has enough available quantity.")
        {
        }
    }
}
