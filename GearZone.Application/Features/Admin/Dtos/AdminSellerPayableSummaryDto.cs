using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminSellerPayableSummaryDto
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
    }
}
