using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Reviews.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api;

[Authorize]
public class ReviewsController : BaseApiController
{
    private readonly IProductReviewService _reviewService;
    private readonly IAuthService _authService;

    public ReviewsController(IProductReviewService reviewService, IAuthService authService)
    {
        _reviewService = reviewService;
        _authService = authService;
    }

    // GET /api/reviews/editor/{orderItemId}
    [HttpGet("editor/{orderItemId:guid}")]
    public async Task<IActionResult> GetEditor(Guid orderItemId)
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var item = await _reviewService.GetReviewEditorAsync(user.Id, orderItemId);
        if (item == null) return FailResponse("This product is not eligible for review.", 404);

        return OkResponse(item);
    }

    // POST /api/reviews
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CreateOrUpdateProductReviewDto request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("Unauthorized.", 401);

        var result = await _reviewService.CreateOrUpdateReviewAsync(user.Id, request);
        if (!result.Succeeded) return FailResponse(result.Message);

        return OkResponse(result.Message);
    }
}
