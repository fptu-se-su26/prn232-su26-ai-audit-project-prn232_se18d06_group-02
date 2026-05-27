using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Shipping.Dtos
{
    public class ShippingFeeCalculationResponseDto
    {
        public decimal TotalShippingFee { get; set; }
        public List<StoreShippingFeeDto> StoreFees { get; set; } = new();
    }

    public class StoreShippingFeeDto
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal ShippingFee { get; set; }
        public double DistanceKm { get; set; }
        public bool IsFreeShipping { get; set; }
    }
}
