using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Seller.Dtos
{
    /// <summary>
    /// What the registration wizard needs to render: the applicant's existing store (if any)
    /// plus any saved draft progress.
    /// </summary>
    public class SellerRegistrationStateDto
    {
        public SellerRegistrationStoreStateDto? ExistingStore { get; set; }
        public SellerRegistrationProgressStateDto? Progress { get; set; }
    }

    public class SellerRegistrationStoreStateDto
    {
        public Guid Id { get; set; }
        public StoreStatus Status { get; set; }
        public string? RejectReason { get; set; }
    }

    public class SellerRegistrationProgressStateDto
    {
        public Guid? StoreId { get; set; }
        public int CurrentStep { get; set; } = 1;
        public Step1Dto Step1 { get; set; } = new();
        public SellerRegistrationStep2StateDto Step2 { get; set; } = new();
        public Step3Dto Step3 { get; set; } = new();

        public static SellerRegistrationProgressStateDto From(RegistrationProgressDto progress) => new()
        {
            StoreId = progress.StoreId,
            CurrentStep = progress.CurrentStep,
            Step1 = progress.Step1,
            Step2 = SellerRegistrationStep2StateDto.From(progress.Step2),
            Step3 = progress.Step3
        };
    }

    /// <summary>
    /// JSON-safe mirror of <see cref="Step2Dto"/>. Step2Dto carries the raw IFormFile uploads,
    /// which System.Text.Json cannot deserialize, so saved state travels as image URLs only.
    /// </summary>
    public class SellerRegistrationStep2StateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime? IdentityIssuedDate { get; set; }
        public string IdentityIssuedPlace { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string? IdentityCardFrontImageUrl { get; set; }
        public string? IdentityCardBackImageUrl { get; set; }

        public static SellerRegistrationStep2StateDto From(Step2Dto dto) => new()
        {
            FullName = dto.FullName,
            IdentityNumber = dto.IdentityNumber,
            IdentityIssuedDate = dto.IdentityIssuedDate,
            IdentityIssuedPlace = dto.IdentityIssuedPlace,
            TaxCode = dto.TaxCode,
            IdentityCardFrontImageUrl = dto.IdentityCardFrontImageUrl,
            IdentityCardBackImageUrl = dto.IdentityCardBackImageUrl
        };

        public Step2Dto ToInput() => new()
        {
            FullName = FullName,
            IdentityNumber = IdentityNumber,
            IdentityIssuedDate = IdentityIssuedDate,
            IdentityIssuedPlace = IdentityIssuedPlace,
            TaxCode = TaxCode,
            IdentityCardFrontImageUrl = IdentityCardFrontImageUrl,
            IdentityCardBackImageUrl = IdentityCardBackImageUrl
        };
    }
}
