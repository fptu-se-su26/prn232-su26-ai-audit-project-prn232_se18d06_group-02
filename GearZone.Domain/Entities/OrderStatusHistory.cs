using GearZone.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Domain.Entities
{
    public class OrderStatusHistory : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public OrderStatus? OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? ChangedByUserId { get; set; }
        public string? Note { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public ApplicationUser? ChangedByUser { get; set; }
    }
}
