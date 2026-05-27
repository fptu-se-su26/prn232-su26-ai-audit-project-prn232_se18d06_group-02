using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Catalog.DTOs
{
    public class ProductComparisonDto
    {
        public List<ComparisonHeaderDto> Products { get; set; } = new();
        public List<ComparisonRowDto> Rows { get; set; } = new();
    }

    public class ComparisonHeaderDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
    }

    public class ComparisonRowDto
    {
        public int AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public List<string?> Values { get; set; } = new(); // Each index matches Products[index]
        public bool IsDifferent { get; set; }
    }
}
