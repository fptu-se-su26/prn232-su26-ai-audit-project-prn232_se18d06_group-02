using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class Store : Entity<Guid>
    {
        public string OwnerUserId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public BusinessType BusinessType { get; set; }

        public string TaxCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? IdentityCardFrontImageUrl { get; set; }
        public string? IdentityCardBackImageUrl { get; set; }

        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankBin { get; set; } = string.Empty;

        public int RegistrationStep { get; set; } = 1;
        public StoreStatus Status { get; set; }
        public string? RejectReason { get; set; }
        public string? LockReason { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
 
        public ApplicationUser OwnerUser { get; set; } = null!;
        public ICollection<ApplicationUser> Staffs { get; set; } = new List<ApplicationUser>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<PromotionCampaign> PromotionCampaigns { get; set; } = new List<PromotionCampaign>();
        public ICollection<StoreFollow> StoreFollows { get; set; } = new List<StoreFollow>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    }
}
