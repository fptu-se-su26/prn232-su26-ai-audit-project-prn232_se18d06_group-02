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
        return OkResponse(store);
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
