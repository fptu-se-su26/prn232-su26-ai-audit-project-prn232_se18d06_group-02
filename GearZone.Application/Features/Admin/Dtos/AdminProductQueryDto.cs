using System;
using System.Collections.Generic;
using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public Guid? StoreId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool OutOfStock { get; set; }
        /// <summary>
        /// Filter by selected category attribute option IDs.
        /// Products must have at least one variant with a matching attribute value option.
        /// </summary>
        public List<int> AttributeOptionIds { get; set; } = new();
    }
}
