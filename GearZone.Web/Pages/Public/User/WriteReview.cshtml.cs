using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Pages.Public.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Public.User
{
    [Authorize]
    public class WriteReviewModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IProductReviewService _productReviewService;

        public WriteReviewModel(IAuthService authService, IProductReviewService productReviewService)
        {
            _authService = authService;
            _productReviewService = productReviewService;
        }

        [BindProperty]
        public WriteProductReviewViewModel Input { get; set; } = new();

        public EligibleReviewItemDto ReviewItem { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid orderItemId)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var reviewItem = await _productReviewService.GetReviewEditorAsync(user.Id, orderItemId);
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

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            ReviewItem = await _productReviewService.GetReviewEditorAsync(user.Id, Input.OrderItemId) ?? new EligibleReviewItemDto();
            if (ReviewItem.OrderItemId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "This product is no longer eligible for review.";
                return RedirectToPage("/Public/User/Profile", new { tab = "orders", orderStatus = "to_review" });
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _productReviewService.CreateOrUpdateReviewAsync(user.Id, new CreateOrUpdateProductReviewDto
            {
                OrderItemId = Input.OrderItemId,
                Rating = Input.Rating,
                Comment = Input.Comment
            });

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Public/User/Profile", new { tab = "reviews" });
        }
    }
}
