using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// StoreProfileResponse moved to GearZone.Application.Features.Seller.Dtos (shared with the Razor client).

namespace GearZone.Api.Controllers.Seller;

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

    // GET /api/seller/store/reviews?filter=&pageNumber=&pageSize=
    [HttpGet("reviews")]
    public async Task<IActionResult> Reviews([FromQuery] SellerReviewQueryDto query)
    {
        var store = await _storeService.GetStoreByOwnerIdAsync(CurrentUserId!);
        if (store == null) return FailResponse("Store not found.", 404);

        var reviews = await _reviewService.GetStoreReviewsAsync(CurrentUserId!, query);
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
