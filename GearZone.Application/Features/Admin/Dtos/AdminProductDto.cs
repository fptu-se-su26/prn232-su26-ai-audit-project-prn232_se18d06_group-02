using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Active, Suspended, Rejected
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
