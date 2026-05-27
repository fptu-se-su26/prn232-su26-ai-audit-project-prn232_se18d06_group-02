using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class ProductImage : Entity<Guid>
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }

        // Navigation
        public Product Product { get; set; } = null!;
    }

}
