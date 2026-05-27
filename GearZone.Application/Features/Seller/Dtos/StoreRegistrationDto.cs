using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class StoreRegistrationDto
    {
        public string OwnerUserId { get; set; } = string.Empty;

        // Step 1: Store Info
        public string StoreName { get; set; } = string.Empty;
        public BusinessType BusinessType { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;

        // Step 2: Identity Verification
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? IdentityIssuedDate { get; set; }
        public string IdentityIssuedPlace { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;

        // Step 3: Banking
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankBin { get; set; } = string.Empty;
    }

    // DTOs for individual steps
    public class Step1Dto
    {
        public string StoreName { get; set; } = string.Empty;
        public BusinessType BusinessType { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
    }

    public class Step2Dto
    {
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? IdentityIssuedDate { get; set; }
        public string IdentityIssuedPlace { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public IFormFile? IdentityCardFrontImage { get; set; }
        public IFormFile? IdentityCardBackImage { get; set; }
        public string? IdentityCardFrontImageUrl { get; set; }
        public string? IdentityCardBackImageUrl { get; set; }
    }

    public class Step3Dto
    {
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankAccountName { get; set; } = string.Empty;
        public string BankBin { get; set; } = string.Empty;
    }

    public class RegistrationProgressDto
    {
        public Guid? StoreId { get; set; }
        public int CurrentStep { get; set; } = 1;
        public Step1Dto Step1 { get; set; } = new();
        public Step2Dto Step2 { get; set; } = new();
        public Step3Dto Step3 { get; set; } = new();
    }
}
