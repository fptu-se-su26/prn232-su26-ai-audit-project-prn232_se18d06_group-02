using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GearZone.Domain.Entities
{
    public class Shipment : Entity<Guid>
    {
        public Guid OrderId { get; set; }
        public Guid StoreId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingDiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetShippingFee { get; set; }
        
        public double DistanceKm { get; set; }
        
        public string? TrackingNumber { get; set; }
        public string? ShippingProvider { get; set; }
        
        // Navigation
        public Order Order { get; set; } = null!;
        public Store Store { get; set; } = null!;
    }
}
