using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;

namespace GearZone.Web.Pages.Public.Catalog.Models
{
    public class ProductReviewPanelViewModel
    {
        public string ProductSlug { get; set; } = string.Empty;
        public ProductReviewSummaryDto ReviewSummary { get; set; } = new();
        public EligibleReviewItemDto? EligibleReview { get; set; }
        public PagedResult<ProductReviewListItemDto> ProductReviews { get; set; } = new();
        public int? ReviewRating { get; set; }
        public bool WithCommentOnly { get; set; }
    }
}
