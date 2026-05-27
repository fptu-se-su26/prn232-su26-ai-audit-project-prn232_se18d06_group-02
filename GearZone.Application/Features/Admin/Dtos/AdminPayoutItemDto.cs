using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutItemDto
    {
        public Guid Id { get; set; }
        public Guid SubOrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public bool IsExcluded { get; set; }
        public string? ExcludeReason { get; set; }
        public DateTime OrderCreatedAt { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }
}
