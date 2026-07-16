using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Pages.Public.User.Models;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.User
{
    [Authorize]
    public class WriteReviewModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the review service in-process.
        private readonly IApiClient _api;

        public WriteReviewModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty]
        public WriteProductReviewViewModel Input { get; set; } = new();

        public EligibleReviewItemDto ReviewItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid orderItemId, CancellationToken ct)
        {
            var reviewItem = await _api.GetAsync<EligibleReviewItemDto>($"/api/reviews/editor/{orderItemId}", ct);
            if (reviewItem == null)
            {
                TempData["ErrorMessage"] = "This product is no longer eligible for review.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders", orderStatus = "to_review" });
            }

            ReviewItem = reviewItem;
            Input = new WriteProductReviewViewModel
            {
                OrderItemId = reviewItem.OrderItemId,
                Rating = reviewItem.ExistingRating ?? 0,
                Comment = reviewItem.ExistingComment
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            ReviewItem = await _api.GetAsync<EligibleReviewItemDto>($"/api/reviews/editor/{Input.OrderItemId}", ct)
                ?? new EligibleReviewItemDto();

            if (ReviewItem.OrderItemId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "This product is no longer eligible for review.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders", orderStatus = "to_review" });
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _api.PostAsync("/api/reviews", new CreateOrUpdateProductReviewDto
            {
                OrderItemId = Input.OrderItemId,
                Rating = Input.Rating,
                Comment = Input.Comment
            }, ct);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.FirstError ?? "Failed to submit review.");
                return Page();
            }

            TempData["SuccessMessage"] = "Your review has been submitted.";
            return RedirectToPage("/Public/User/Profile", new { tab = "reviews" });
        }
    }
}
