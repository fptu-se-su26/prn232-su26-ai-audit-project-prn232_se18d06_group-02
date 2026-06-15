using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Controllers.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api.Seller;

[Authorize(Roles = "Store Owner")]
[Route("api/seller/store")]
[ApiController]
public class StoreSettingsController : BaseApiController
{
    private readonly ISellerStoreService _storeService;
    private readonly IProductReviewService _reviewService;

    public StoreSettingsController(ISellerStoreService storeService, IProductReviewService reviewService)
    {
        _storeService = storeService;
        _reviewService = reviewService;
    }

    // GET /api/seller/store
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);
        return OkResponse(new StoreProfileResponse
        {
            Id = store.Id,
            OwnerUserId = store.OwnerUserId,
            StoreName = store.StoreName,
            Slug = store.Slug,
            Description = store.Description,
            LogoUrl = store.LogoUrl,
            BusinessType = store.BusinessType.ToString(),
            TaxCode = store.TaxCode,
            Phone = store.Phone,
            Email = store.Email,
            AddressLine = store.AddressLine,
            Province = store.Province,
            Latitude = store.Latitude,
            Longitude = store.Longitude,
            BankAccountNumber = store.BankAccountNumber,
            BankAccountName = store.BankAccountName,
            BankName = store.BankName,
            BankBin = store.BankBin,
            RegistrationStep = store.RegistrationStep,
            Status = store.Status.ToString(),
            RejectReason = store.RejectReason,
            LockReason = store.LockReason,
            CommissionRate = store.CommissionRate,
            CreatedAt = store.CreatedAt,
            ApprovedAt = store.ApprovedAt,
            UpdatedAt = store.UpdatedAt
        });
    }

    // PUT /api/seller/store
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateStoreProfileDto dto)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var result = await _storeService.UpdateStoreProfileAsync(CurrentUserId!, dto);
        return result
            ? OkResponse("Store profile updated.")
            : FailResponse("Failed to update. Only approved stores can update their profile.");
    }

    // GET /api/seller/store/reviews
    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews([FromQuery] int page = 1)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        var reviews = await _reviewService.GetStoreReviewsAsync(CurrentUserId!, new GearZone.Application.Features.Reviews.Dtos.SellerReviewQueryDto { PageNumber = page });
        return OkResponse(reviews);
    }

    // POST /api/seller/store/reviews/{reviewId}/reply
    [HttpPost("reviews/{reviewId:guid}/reply")]
    public async Task<IActionResult> ReplyToReview(Guid reviewId, [FromBody] ReviewReplyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReplyContent))
            return FailResponse("Reply content cannot be empty.");

        var result = await _reviewService.ReplyAsync(CurrentUserId!, reviewId, request.ReplyContent);
        return result.Succeeded
            ? OkResponse("Reply posted.")
            : FailResponse(result.Message.Length > 0 ? result.Message : "Failed to post reply.");
    }
}

public class ReviewReplyRequest
{
    public string ReplyContent { get; set; } = string.Empty;
}

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
