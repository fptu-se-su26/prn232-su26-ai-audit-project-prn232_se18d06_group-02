using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class Cart : Entity<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

}
