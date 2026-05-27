using GearZone.Domain.Enums;
using System;

namespace GearZone.Application.Features.Admin.Dtos;

public class StoreApplicationDto
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public decimal CommissionRate { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string BankBin { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public string? IdentityNumber { get; set; }
    public DateTime? IdentityIssuedDate { get; set; }
    public string? IdentityIssuedPlace { get; set; }
    public string? IdentityCardFrontImageUrl { get; set; }
    public string? IdentityCardBackImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? RejectReason { get; set; }
    public StoreStatus Status { get; set; }
}
