using GearZone.Application.Common.Models;
using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class PlatformTransactionQuery : PaginationRequest
    {
        public string? Search { get; set; }
        public string? Type { get; set; } // "Payment", "Payout", "Topup", "Adjustment", "Refund"
        public string? Status { get; set; }
        public string? StoreName { get; set; }
        public string? BatchCode { get; set; }
        public string? DateRangeType { get; set; } // "Today", "Week", "Month", "Custom"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? Direction { get; set; } // "IN", "OUT"
    }
}
