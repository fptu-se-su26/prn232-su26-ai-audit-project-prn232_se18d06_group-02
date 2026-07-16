using System.Security.Claims;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Reviews
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the review service in-process.
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public SellerReviewQueryDto Query { get; set; } = new();

        public PagedResult<SellerReviewListItemDto> Reviews { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var filter = string.IsNullOrWhiteSpace(Query.Filter) ? "all" : Query.Filter;
            var pageNumber = Query.PageNumber < 1 ? 1 : Query.PageNumber;

            var data = await _api.GetAsync<PagedResult<SellerReviewListItemDto>>(
                $"/api/seller/store/reviews?filter={Uri.EscapeDataString(filter)}&pageNumber={pageNumber}", ct);
            if (data is not null)
            {
                Reviews = data;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostReplyAsync(Guid reviewId, string replyContent, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return RedirectToPage("/Public/Auth/Login");
            }

            var result = await _api.PostAsync($"/api/seller/store/reviews/{reviewId}/reply", new { replyContent }, ct);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.FirstError ?? "Reply posted.";

            return RedirectToPage(new
            {
                filter = Query.Filter,
                pageNumber = Query.PageNumber
            });
        }
    }
}
