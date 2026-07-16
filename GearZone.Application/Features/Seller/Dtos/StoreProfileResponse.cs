using System;

namespace GearZone.Application.Features.Seller.Dtos
{
    /// <summary>Store profile payload shared by the API and the Razor settings page.</summary>
    public class StoreProfileResponse
    {
        public Guid Id { get; set; }
        public string OwnerUserId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string BusinessType { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankBin { get; set; } = string.Empty;
        public int RegistrationStep { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectReason { get; set; }
        public string? LockReason { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
