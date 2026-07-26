namespace GearZone.Application.Features.Checkout.Dtos
{
    public class VoucherEvaluationContextDto
    {
        public List<VoucherEvaluationLineDto> Lines { get; set; } = new();
        public List<VoucherShippingFeeDto> ShippingFees { get; set; } = new();

        public decimal MerchandiseTotal => Lines.Sum(x => x.EffectiveSubtotal);
        public decimal ShippingTotal => ShippingFees.Sum(x => x.ShippingFee);
    }

    public class VoucherEvaluationLineDto
    {
        public Guid StoreId { get; set; }
        public int CategoryId { get; set; }
        public decimal EffectiveSubtotal { get; set; }
    }

    public class VoucherShippingFeeDto
    {
        public Guid StoreId { get; set; }
        public decimal ShippingFee { get; set; }
    }
}
