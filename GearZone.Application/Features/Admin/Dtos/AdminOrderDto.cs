using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminOrderDto
    {
        public Guid Id { get; set; }
        public long OrderCode { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
