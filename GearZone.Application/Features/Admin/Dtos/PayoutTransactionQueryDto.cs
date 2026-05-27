using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;
using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class PayoutTransactionQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public PayoutTransactionStatus? Status { get; set; }
        public string? DateRangeType { get; set; } // "Today", "Week", "Month", "Custom"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
