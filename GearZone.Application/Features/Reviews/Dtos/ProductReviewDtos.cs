using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Reviews.Dtos
{
    public class CreateOrUpdateProductReviewDto
    {
        public Guid OrderItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class ProductReviewQueryDto
    {
        public int? Rating { get; set; }
        public bool WithCommentOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }

    public class SellerReviewQueryDto
    {
        public string Filter { get; set; } = "all";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ProductReviewSummaryDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int WithCommentCount { get; set; }
        public List<ProductReviewBreakdownDto> Breakdown { get; set; } = new();
    }

    public class ProductReviewBreakdownDto
    {
        public int Rating { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ProductReviewListItemDto
    {
        public Guid Id { get; set; }
        public Guid OrderItemId { get; set; }
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? SellerReplyContent { get; set; }
        public DateTime? SellerReplyAt { get; set; }
        public DateTime? SellerReplyUpdatedAt { get; set; }
    }

    public class EligibleReviewItemDto
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public long OrderCode { get; set; }
        public DateTime DeliveredAt { get; set; }
        public DateTime ReviewDeadline { get; set; }
        public bool HasExistingReview { get; set; }
        public Guid? ReviewId { get; set; }
        public int? ExistingRating { get; set; }
        public string? ExistingComment { get; set; }
    }

    public class MyReviewDto
    {
        public Guid Id { get; set; }
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime DeliveredAt { get; set; }
        public DateTime ReviewDeadline { get; set; }
        public bool CanEdit { get; set; }
        public string? SellerReplyContent { get; set; }
        public DateTime? SellerReplyAt { get; set; }
    }

    public class SellerReviewListItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SellerReplyContent { get; set; }
        public DateTime? SellerReplyAt { get; set; }
    }

    public class StoreReviewSnapshotDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class ReviewOperationResultDto
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ReviewOperationResultDto Success(string message) => new() { Succeeded = true, Message = message };
        public static ReviewOperationResultDto Failure(string message) => new() { Succeeded = false, Message = message };
    }
}
