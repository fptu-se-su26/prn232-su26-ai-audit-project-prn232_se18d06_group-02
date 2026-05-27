using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Reviews
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly IProductReviewService _productReviewService;
        private readonly IAuthService _authService;

        public IndexModel(IProductReviewService productReviewService, IAuthService authService)
        {
            _productReviewService = productReviewService;
            _authService = authService;
        }

        [BindProperty(SupportsGet = true)]
        public SellerReviewQueryDto Query { get; set; } = new();

        public PagedResult<SellerReviewListItemDto> Reviews { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            Reviews = await _productReviewService.GetStoreReviewsAsync(user.Id, Query);
            return Page();
        }

        public async Task<IActionResult> OnPostReplyAsync(Guid reviewId, string replyContent)
        {
            var user = await _authService.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var result = await _productReviewService.ReplyAsync(user.Id, reviewId, replyContent);
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToPage(new
            {
                filter = Query.Filter,
                pageNumber = Query.PageNumber
            });
        }
    }
}
