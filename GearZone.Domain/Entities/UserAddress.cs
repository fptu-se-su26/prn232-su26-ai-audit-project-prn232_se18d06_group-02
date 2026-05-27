using GearZone.Domain.Enums;
using System;

namespace GearZone.Domain.Entities
{
    public class UserAddress : Entity<Guid>
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string AddressLine { get; set; } = string.Empty; // Full address
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        
        public AddressType AddressType { get; set; } = AddressType.Home;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
