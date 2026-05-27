using Microsoft.AspNetCore.Identity;

namespace GearZone.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public string? AvatarUrl { get; set; }

        public string? IdentityNumber { get; set; } // CCCD
        public DateTime? IdentityIssuedDate { get; set; }
        public string? IdentityIssuedPlace { get; set; }


        public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
        
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public ICollection<Store> OwnedStores { get; set; } = new List<Store>();
        public ICollection<Store> StaffStores { get; set; } = new List<Store>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<StoreFollow> StoreFollows { get; set; } = new List<StoreFollow>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}
  
