using GearZone.Domain.Enums;
using System.Collections.Generic;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutTransactionSummaryDto
    {
        public decimal TotalGrossRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal TotalNetDisbursed { get; set; }
        public int TotalTransactions { get; set; }
        
        public int PendingCount { get; set; }
        public int ProcessingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
    }
}
