using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;
using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPayoutBatchQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public PayoutBatchStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DateRange { get; set; }
    }
}
