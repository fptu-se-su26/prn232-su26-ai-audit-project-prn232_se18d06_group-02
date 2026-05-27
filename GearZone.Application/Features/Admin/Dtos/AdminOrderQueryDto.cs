using System;
using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminOrderQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public bool? IsPaid { get; set; }
        public string? PaymentMethod { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? DateRange { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
