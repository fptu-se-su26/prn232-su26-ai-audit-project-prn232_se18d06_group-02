using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Checkout.Dtos
{
    public class CheckoutRequestDto
    {
        public Guid RequestId { get; set; }
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
        public ShippingInfoDto ShippingInfo { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public bool SaveAddress { get; set; }
        public bool IsDefaultAddress { get; set; }
        public string? OrderVoucherCode { get; set; }
        public string? ShippingVoucherCode { get; set; }
    }

    public class ShippingInfoDto
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public AddressType AddressType { get; set; } = AddressType.Home;
    }

    public class CheckoutResponseDto
    {
        public bool Success { get; set; }
        public bool IsConflict { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? ErrorMessage { get; set; }
        
        public string? Bin { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public long? Amount { get; set; }
        public string? Description { get; set; }
        public string? QrCode { get; set; }
    }
}
