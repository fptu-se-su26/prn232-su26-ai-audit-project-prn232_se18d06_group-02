using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutBatchSummaryDto
    {
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public int BatchCount { get; set; }
    }
}
